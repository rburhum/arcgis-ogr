/******************************************************************************
 * $Id$
 *
 * Project:  GDAL/OGR ArcGIS Datasource
 * Purpose:  Dataset Implementation for ArcGIS
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

using OSGeo.OGR;

namespace GDAL.OGRPlugin
{
    [ComVisible(false)]
    internal class OGRDataset : IPlugInDatasetHelper, IPlugInDatasetInfo, IPlugInFastRowCount
    {
        
        private string m_fullPath;

        private OSGeo.OGR.Layer m_layer;

        private IFields m_fields;
        private esriDatasetType m_datasetType;
        private esriGeometryType m_geometryType;
        private ESRI.ArcGIS.Geometry.ISpatialReference m_spatialReference;
        private int m_geometryFieldIndex;
        private int m_oidFieldIndex;
        private System.Collections.Hashtable m_fieldMapping;
        

        public OGRDataset(OSGeo.OGR.Layer layer)
        {
            m_layer = layer;   
         
            ogr_utils.map_fields(layer, out m_fieldMapping, out m_fields, out m_datasetType, 
                                out m_geometryType, out m_geometryFieldIndex, out m_oidFieldIndex, 
                                out m_spatialReference);

        }

        #region IPlugInDatasetHelper Members

        public IEnvelope Bounds
        {
            get
            {
                if (this.DatasetType == esriDatasetType.esriDTTable)
                    return null;

                OSGeo.OGR.Envelope ogrEnvelope = new OSGeo.OGR.Envelope();
                m_layer.GetExtent(ogrEnvelope,0);

                return ogr_utils.get_extent(ogrEnvelope, m_spatialReference);
            }
        }

        public int get_ShapeFieldIndex(int ClassIndex)
        {
            return m_geometryFieldIndex;
        }

        public IFields get_Fields(int ClassIndex)
        {
            return m_fields;
        }

        public string get_ClassName(int Index)
        {
            return ogr_utils.field_to_description(m_fields.get_Field(m_geometryFieldIndex));
        }

        public int get_OIDFieldIndex(int ClassIndex)
        {
            return m_oidFieldIndex;
        }

        public int ClassCount
        {
            get
            {
                return 1;
            }
        }

        public int get_ClassIndex(string Name)
        {
            return 0;
        }


        #region Fetching - returns cursor //HIGHLIGHT: Fetching
        public IPlugInCursorHelper FetchAll(int ClassIndex, string WhereClause, object FieldMap)
        {
            return null;
            /*
            try
            {
                OGRCursor allCursor =
                    new OGRCursor(m_fullPath, this.get_Fields(ClassIndex), -1,
                    (System.Array)FieldMap, null, this.geometryTypeByID(ClassIndex));
                setMZ(allCursor, ClassIndex);
                return (IPlugInCursorHelper)allCursor;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
             */
        }

        public IPlugInCursorHelper FetchByID(int ClassIndex, int ID, object FieldMap)
        {
            return null;
            /*
            try
            {
                OGRCursor idCursor =
                    new OGRCursor(m_fullPath, this.get_Fields(ClassIndex), ID,
                    (System.Array)FieldMap, null, this.geometryTypeByID(ClassIndex));

                setMZ(idCursor, ClassIndex);
                return (IPlugInCursorHelper)idCursor;
            }
            catch (Exception ex)	//will catch NextRecord error if it reaches EOF without finding a record
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
             * */
        }

        public IPlugInCursorHelper FetchByEnvelope(int ClassIndex, IEnvelope env, bool strictSearch, string WhereClause, object FieldMap)
        {
            return null;
            /*
             
            if (this.DatasetType == esriDatasetType.esriDTTable)
                return null;

            //env passed in always has same spatial reference as the data
            //for identify, it will check if search geometry intersect dataset bound
            //but not ITable.Search(pSpatialQueryFilter, bRecycle) etc
            //so here we should check if input env falls within extent
            IEnvelope boundEnv = this.Bounds;
            boundEnv.Project(env.SpatialReference);
            if (boundEnv.IsEmpty)
                return null;	//or raise error?
            try
            {
                OGRCursor spatialCursor = new OGRCursor(m_fullPath,
                    this.get_Fields(ClassIndex), -1,
                    (System.Array)FieldMap, env, this.geometryTypeByID(ClassIndex));
                setMZ(spatialCursor, ClassIndex);

                return (IPlugInCursorHelper)spatialCursor;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
             */ 
        }

        #endregion


        #endregion

        #region IPlugInDatasetInfo Members
        
        public string LocalDatasetName
        {
            get
            {
                return m_layer.GetName();
            }
        }

        public string ShapeFieldName
        {
            get
            {
                if (m_geometryFieldIndex == -1)
                    return null;
                else
                    return m_fields.get_Field(m_geometryFieldIndex).Name;
                 
            }
        }

        public esriDatasetType DatasetType
        {
            get
            {
                return m_datasetType;                
            }
        }

        public esriGeometryType GeometryType
        {
            get
            {
                return m_geometryType;                
            }
        }

        #endregion

        public int RowCount
        {
            get 
            {
                return 0;
                //return m_layer.GetFeatureCount(0); 
            }
        }
    }
}
