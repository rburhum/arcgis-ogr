using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Geometry;

namespace GDAL.OGRPlugin
{
    class ogr_utils
    {
        public static ESRI.ArcGIS.Geometry.IEnvelope get_extent(OSGeo.OGR.Envelope ogr_envelope)
        {
            return null;
        }

        public static String field_to_description(ESRI.ArcGIS.Geodatabase.IField field)
        {
            return "";

            /*
              if (Index % 3 == 0)
                m_datasetString = "Point";
            if (Index % 3 == 1)
                m_datasetString = "Polyline";
            if (Index % 3 == 2)
                m_datasetString = "Polygon";

            if ((Index >= 3 && Index < 6) || Index >= 9)
                m_datasetString += "M";

            if (Index >= 6)
                m_datasetString += "Z"
             
             */ 

        }

        public static void map_fields(OSGeo.OGR.Layer ogr_layer, 
            out ESRI.ArcGIS.Geodatabase.IFields outFields, 
            out ESRI.ArcGIS.Geodatabase.esriDatasetType outDatasetType,
            out ESRI.ArcGIS.Geometry.esriGeometryType outGeometryType,
            out int outShapeIndex,
            out int outOIDFieldIndex)
        {
            outFields = null;
            outDatasetType = ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTFeatureClass;
            outGeometryType = ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint;
            outShapeIndex = 1;
            outOIDFieldIndex = 0;
            /*
             //singleton
                ISpatialReferenceFactory2 srefFact = new SpatialReferenceEnvironmentClass();
                return srefFact.CreateProjectedCoordinateSystem
                    (Convert.ToInt32(esriSRProjCSType.esriSRProjCS_World_Robinson));	// WGS1984UTM_10N)); 
              
             
            IFieldEdit fieldEdit;
            IFields fields;
            IFieldsEdit fieldsEdit;
            IObjectClassDescription fcDesc;
            if (this.DatasetType == esriDatasetType.esriDTTable)
                fcDesc = new ObjectClassDescriptionClass();
            else
                fcDesc = new FeatureClassDescriptionClass();

            fields = fcDesc.RequiredFields;
            fieldsEdit = (IFieldsEdit)fields;

            fieldEdit = new FieldClass();
            fieldEdit.Length_2 = 1;
            fieldEdit.Name_2 = "ColumnOne";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldsEdit.AddField((IField)fieldEdit);

            //HIGHLIGHT: Add extra int column
            fieldEdit = new FieldClass();
            fieldEdit.Name_2 = "Extra";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldsEdit.AddField((IField)fieldEdit);

            //HIGHLIGHT: Set shape field geometry definition
            if (this.DatasetType != esriDatasetType.esriDTTable)
            {
                IField field = fields.get_Field(fields.FindField("Shape"));
                fieldEdit = (IFieldEdit)field;
                IGeometryDefEdit geomDefEdit = (IGeometryDefEdit)field.GeometryDef;
                geomDefEdit.GeometryType_2 = geometryTypeByID(ClassIndex);
                ISpatialReference shapeSRef = this.spatialReference;

                #region M & Z
                //M
                if ((ClassIndex >= 3 && ClassIndex <= 5) || ClassIndex >= 9)
                {
                    geomDefEdit.HasM_2 = true;
                    shapeSRef.SetMDomain(0, 1000);
                }
                else
                    geomDefEdit.HasM_2 = false;

                //Z
                if (ClassIndex >= 6)
                {
                    geomDefEdit.HasZ_2 = true;
                    shapeSRef.SetZDomain(0, 1000);
                }
                else
                    geomDefEdit.HasZ_2 = false;
                #endregion

                geomDefEdit.SpatialReference_2 = shapeSRef;
            }

            return fields;
             */
        }
    }
}
