/******************************************************************************
 * $Id$
 *
 * Project:  GDAL/OGR ArcGIS Datasource
 * Purpose:  Cursor Implementation for ArcGIS
 * Author:   Ragi Yaser Burhum, ragi@burhum.com
 *
 ******************************************************************************
 * Copyright (c) 2012, Ragi Yaser Burhum
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace GDAL.OGRPlugin
{
    [ComVisible(false)]
    internal class OGRCursor : IPlugInCursorHelper
    {
        private bool m_isFinished;
        
        private OGRDataset m_pDataset;

        private String m_whereClause;
        
        //m_esriQueryFieldMap is an array passed by the ArcObjects framework that contains
        //an array that we should use to limit the columns we should return. it is an optimization
        //on the ArcObjects side to avoid fetching the entire row. Honestly, it is a mindf*ck because
        //it is mapping values that are already mapped.
        //
        private System.Array m_esriQueryFieldMap;

        private IEnvelope m_envelope;

        private OSGeo.OGR.Feature m_currentOGRFeature;

        private int m_onlyOneID; // when this is set, we only retrieve this one id
        
        #region HRESULT definitions
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int S_FALSE = 1;
        #endregion



        public OGRCursor(OGRDataset parent, String whereClause, System.Array esriQueryFieldMap, IEnvelope env = null, int onlyOneID = -1)
        {
            
            m_isFinished = false;
            m_pDataset = parent;
            
            m_whereClause = whereClause;

            if (m_whereClause != null && m_whereClause.Length > 0)
                m_pDataset.ogrLayer.SetAttributeFilter(m_whereClause);
            else
                m_pDataset.ogrLayer.SetAttributeFilter(null);

            m_envelope = env;
            if (m_envelope != null)
            {
                m_envelope.Project(m_pDataset.SpatialReference);
                m_pDataset.ogrLayer.SetSpatialFilterRect(m_envelope.XMin, m_envelope.YMin, m_envelope.XMax, m_envelope.YMax);
            }
            else
            {
                m_pDataset.ogrLayer.SetSpatialFilter(null);
            }


            m_esriQueryFieldMap = esriQueryFieldMap;

                       
            m_pDataset.ogrLayer.ResetReading();

            m_currentOGRFeature = null;

            m_onlyOneID = onlyOneID;

            if (m_onlyOneID == -1)
                NextRecord();
            else
            {
                RetrieveOnlyOneRecord();
            }
        }

        #region IPlugInCursorHelper Members

        public int QueryValues(IRowBuffer row)
        {
            try
            {
                if (m_currentOGRFeature == null)
                    return -1;

                // Loop and only set values in the rowbuffer
                // that were requested by ArcGIS.
                // 
                // Ignore the ArcObjects documentation because it says I should use the field map
                // and that is straight out incorrect. Only copy values if the fieldmap has any value
                // besides -1. Othewise, ignore whatever map it is asking to copy to and simply copy
                // to the right field.
               

                IFields pFields = m_pDataset.get_Fields(0);
                int fieldCount = pFields.FieldCount;

                for (int i = 0; i < fieldCount; i++)
                {
                    int esriFieldIndex = (int)m_esriQueryFieldMap.GetValue(i);

                    if (esriFieldIndex == -1 ||
                        i == m_pDataset.get_OIDFieldIndex(0) ||
                        i == m_pDataset.get_ShapeFieldIndex(0))
                        continue; 

                    IField pField = pFields.get_Field(i);
                    object val = null;

                    // DANGER - POTENTIAL BUG - Workaround. For some reason, in ArcGIS 10.1SP1 I am
                    // getting fields that should not be editable passed in to map into!!!?!? I am 
                    // skipping those here. In theory, since we are the DataSource provider, we should be able to
                    // use a lower level set value that skips polymorphic behavior and hence allows
                    // the write to happen even in non-editable fields. Something analogous to 
                    // ITableWrite::WriteRow, but for Rows. We don't have that, so I skip those values for those 
                    // rows.
                    if (!pFields.get_Field(i).Editable)
                        continue;

                    try
                    {
                        val = m_pDataset.get_mapped_value(m_currentOGRFeature, i);

                        row.set_Value(i, val);
                    }
                    catch (Exception ex)
                    {
                        // skip values that fail to be set but continue doing it anyway
                        string msg = String.Format("OGRFID:[{0}] esriFieldName:[{1}] esriFieldIndex:[{2}] esriValue:[{3}] Exception:[{4}]",
                              m_currentOGRFeature.GetFID(),
                              pField.Name,
                              esriFieldIndex,
                              val != null ? val.ToString() : "<not set>",
                              ex.Message);
                        System.Diagnostics.Debug.WriteLine(msg);
                    }
                }

                return m_currentOGRFeature.GetFID();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return -1;
            }

        }

        public void QueryShape(IGeometry pGeometry)
        {
            if (pGeometry == null)
                return;

            try
            {
                OSGeo.OGR.Geometry ogrGeometry = m_currentOGRFeature.GetGeometryRef();

                // Flatten the geometry and ommit Z value until we add manual 
                // Z-value zupport
                // See: 
                // https://github.com/RBURHUM/arcgis-ogr/issues/11
                // 
                //
                ogrGeometry.FlattenTo2D();

                //export geometry from OGR to WKB
                int wkbSize = ogrGeometry.WkbSize();
                byte[] wkbBuffer = new byte[wkbSize];
                ogrGeometry.ExportToWkb(wkbBuffer);

                //import geometry from WKB to ESRI Shape
                IWkb pWKB = pGeometry as IWkb;
                pWKB.ImportFromWkb(wkbSize, ref wkbBuffer[0]);

                pGeometry.SpatialReference = m_pDataset.SpatialReference;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(" Error: " + ex.Message);
                pGeometry.SetEmpty();
            }
        }

        
        public bool IsFinished()
        {
            return m_isFinished;
        }

        public void NextRecord()
        {
            if (m_isFinished)	//error already thrown once
                return;

            m_currentOGRFeature = m_pDataset.ogrLayer.GetNextFeature();

            if (m_currentOGRFeature == null)
            {
                m_isFinished = true;

                throw new COMException("End of OGR Plugin cursor", S_FALSE);             
            }

            return;

        }
        #endregion

        public void RetrieveOnlyOneRecord()
        {
            m_currentOGRFeature = m_pDataset.ogrLayer.GetFeature(m_onlyOneID);
            m_isFinished = true;
        }
    }
}
        
