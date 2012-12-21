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

                // loop and only set values in the rowbuffer
                // that were requested from the fieldMap

                int count = m_esriQueryFieldMap.GetLength(0);

                for (int i = 0; i < count; i++)
                {
                    int esriFieldIndex = (int)m_esriQueryFieldMap.GetValue(i);

                    // have to skip 1) objectid and 2) geometry field or we will get an ESRI
                    // exception. Also have to skip anything that the query map is asking to ignore

                    if (esriFieldIndex == -1 || 
                        esriFieldIndex == m_pDataset.get_OIDFieldIndex(0) ||
                        esriFieldIndex == m_pDataset.get_ShapeFieldIndex(0))
                        continue;

                    try
                    {

                        IField valField = m_pDataset.get_Fields(0).get_Field(i);

                        object val = m_pDataset.get_mapped_value(m_currentOGRFeature, i);

                        row.set_Value(i, val);
                    }
                    catch (Exception ex)
                    {
                        // skip values that fail to be set but continue doing it anyway
                        System.Diagnostics.Debug.WriteLine(ex.Message);
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

                // It seems that the IWkb.ImportFromWkb called below has trouble with Z values.
                // Will need to investigate this further, but as a quick fix, we flatten
                // the geometry prior to conversion.
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
        
