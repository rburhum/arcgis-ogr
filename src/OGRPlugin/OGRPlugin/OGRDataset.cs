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
        
        private OSGeo.OGR.Layer m_layer;
        private IFields m_fields;
        private esriDatasetType m_datasetType;
        private esriGeometryType m_geometryType;
        private ESRI.ArcGIS.Geometry.ISpatialReference m_spatialReference;
        private int m_geometryFieldIndex;
        private int m_oidFieldIndex;
        private System.Collections.Hashtable m_fieldMapping;

        #region accessors for fields

        public OSGeo.OGR.Layer ogrLayer
        {
            get { return m_layer;}
        }

        public IFields fields
        {
            get { return m_fields; }
        }

        public System.Collections.Hashtable fieldMapping
        {
            get { return m_fieldMapping; }
        }

        public ISpatialReference SpatialReference
        {
            get { return m_spatialReference; }
        }

        #endregion


        public OGRDataset(OSGeo.OGR.Layer layer)
        {
            m_layer = layer;   
         
            ogr_utils.map_fields(layer, out m_fieldMapping, out m_fields, out m_datasetType, 
                                out m_geometryType, out m_geometryFieldIndex, out m_oidFieldIndex, 
                                out m_spatialReference);

        }

        public object get_mapped_value(OSGeo.OGR.Feature feature, int esriFieldsIndex)
        {
            // get the ESRI Field
            ESRI.ArcGIS.Geodatabase.IField pField = m_fields.get_Field(esriFieldsIndex);

            if (esriFieldsIndex == m_oidFieldIndex)
                return feature.GetFID();

            if (esriFieldsIndex == m_geometryFieldIndex)
            {
                System.Diagnostics.Debug.Assert(false);
                return null; // this should never be called for geometries
            }

            int ogrIndex = (int) m_fieldMapping[esriFieldsIndex];

            if (!feature.IsFieldSet(ogrIndex))
                return null;

            switch (feature.GetFieldType(ogrIndex))
            {
                // must be kept in sync with utilities library

                case OSGeo.OGR.FieldType.OFTInteger:
                    return feature.GetFieldAsInteger(ogrIndex);                    
                    
                case OSGeo.OGR.FieldType.OFTReal:
                    return feature.GetFieldAsDouble(ogrIndex);
                    
                case OSGeo.OGR.FieldType.OFTString:
                    return feature.GetFieldAsString(ogrIndex);

                case OSGeo.OGR.FieldType.OFTBinary:
                     
                   // WTF, the C# bindings don't have a blob retrieval until this ticket gets solved
                  // http://trac.osgeo.org/gdal/ticket/4457#comment:2

                    return null;

                case OSGeo.OGR.FieldType.OFTDateTime:
                    {

                        int year, month, day, hour, minute, second, flag;
                        feature.GetFieldAsDateTime(ogrIndex, out year, out month, out day, out hour, out minute, out second, out flag);

                        DateTime date = new DateTime(year, month, day, hour, minute, second);
                        return date;
                    }
                    
                    
                default:
                    return feature.GetFieldAsString(ogrIndex); //most things coerce as strings
            }
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


        #region Fetching - returns cursor
        public IPlugInCursorHelper FetchAll(int ClassIndex, string whereClause, object fieldMap)
        {            
            try
            {
                OGRCursor allCursor = new OGRCursor(this, whereClause, (System.Array) fieldMap, null);                
                return (IPlugInCursorHelper)allCursor;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public IPlugInCursorHelper FetchByID(int ClassIndex, int ID, object fieldMap)
        {
            try
            {
                OGRCursor allCursor = new OGRCursor(this, null, (System.Array)fieldMap, null, ID);
                return (IPlugInCursorHelper)allCursor;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public IPlugInCursorHelper FetchByEnvelope(int ClassIndex, IEnvelope env, bool strictSearch, string whereClause, object fieldMap)
        {
            try
            {
                OGRCursor allCursor = new OGRCursor(this, whereClause, (System.Array)fieldMap, env);
                return (IPlugInCursorHelper)allCursor;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
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
                return m_layer.GetFeatureCount(0); 
            }
        }
    }
}
