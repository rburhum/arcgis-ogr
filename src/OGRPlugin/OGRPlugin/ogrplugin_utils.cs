using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.Geometry;

namespace GDAL.OGRPlugin
{
    class ogr_utils
    {
        public static ESRI.ArcGIS.Geometry.IEnvelope get_extent(OSGeo.OGR.Envelope ogr_envelope, ISpatialReference sr)
        {
            IEnvelope env = new EnvelopeClass();
            env.PutCoords(ogr_envelope.MinX, ogr_envelope.MinY, ogr_envelope.MaxX, ogr_envelope.MaxY);
            env.SpatialReference = sr;

            return env;
        }

        public static String field_to_description(ESRI.ArcGIS.Geodatabase.IField field)
        {
            return "Geometry";

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
            out System.Collections.Hashtable outFieldMap,
            out ESRI.ArcGIS.Geodatabase.IFields outFields, 
            out ESRI.ArcGIS.Geodatabase.esriDatasetType outDatasetType,
            out ESRI.ArcGIS.Geometry.esriGeometryType outGeometryType,
            out int outShapeIndex,
            out int outOIDFieldIndex,
            out ISpatialReference outSpatialReference)
        {

            outSpatialReference = null;
            outFields = null;
            outDatasetType = ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTTable; // start assuming it is a table
            outGeometryType = esriGeometryType.esriGeometryNull; //don't know what it is
            outOIDFieldIndex = -1;
            outShapeIndex = -1;
            outFieldMap = new System.Collections.Hashtable();

            System.Collections.ArrayList fieldArray = new System.Collections.ArrayList();

            OSGeo.OGR.FeatureDefn featDef = ogr_layer.GetLayerDefn();

            int fieldsInserted = 0;

            // OIDs and Geometries can be pseudo fields in GDAL and are thus *may* not included in the OGR FieldDef
            // To account for that add those first (if they exist) and keep a mapping of fields using
            // fieldsInserted


            //////////////////////////////
            //
            // handle oid field pseudo column
            //
            ESRI.ArcGIS.Geodatabase.IFieldEdit2 oidFieldEdit = new ESRI.ArcGIS.Geodatabase.FieldClass();

            if (ogr_layer.GetFIDColumn().Length > 0)
            {
                
                oidFieldEdit.Name_2 = ogr_layer.GetFIDColumn();
                oidFieldEdit.AliasName_2 = ogr_layer.GetFIDColumn();
            }
            else
            {
                oidFieldEdit.Name_2 = "FID";
                oidFieldEdit.AliasName_2 = "FID";
            }

            oidFieldEdit.Type_2 = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeOID;                
            fieldArray.Add(oidFieldEdit);
            outOIDFieldIndex = fieldsInserted;
            fieldsInserted++;            

            //////////////////////////////////////
            //
            // handle (optional) geometry field pseudo column
            //

            if (!(ogr_layer.GetGeomType() == OSGeo.OGR.wkbGeometryType.wkbNone ||
                  ogr_layer.GetGeomType() == OSGeo.OGR.wkbGeometryType.wkbUnknown))
            {
                ESRI.ArcGIS.Geodatabase.IFieldEdit2 geomFieldEdit = new ESRI.ArcGIS.Geodatabase.FieldClass();


                if (ogr_layer.GetGeometryColumn().Length > 0)
                {

                    geomFieldEdit.Name_2 = ogr_layer.GetGeometryColumn();
                    geomFieldEdit.AliasName_2 = ogr_layer.GetGeometryColumn();

                }
                else
                {
                    geomFieldEdit.Name_2 = "Shape";
                    geomFieldEdit.AliasName_2 = "Shape";
                }                

                geomFieldEdit.Type_2 = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeGeometry;

                // add geometry def
               
                ESRI.ArcGIS.Geometry.esriGeometryType gdbType;
                bool hasZ;
                ogr_geo_type_to_esri_geo_type(ogr_layer.GetGeomType(), out gdbType, out hasZ);

                ESRI.ArcGIS.Geodatabase.IGeometryDefEdit geomDef = new ESRI.ArcGIS.Geodatabase.GeometryDefClass();
                geomDef.GeometryType_2 = gdbType;
                geomDef.HasM_2 = false; //no M support on OGR
                geomDef.HasZ_2 = hasZ;
                geomDef.SpatialReference_2 = outSpatialReference = ogr_utils.get_spatialReference(ogr_layer.GetSpatialRef());

                geomFieldEdit.GeometryDef_2 = geomDef;

                fieldArray.Add(geomFieldEdit);                

                outDatasetType = ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTFeatureClass; // upgrade to featureclass
                outGeometryType = gdbType;
                outShapeIndex = fieldsInserted;

                fieldsInserted++;
            }

            int fieldCount = featDef.GetFieldCount();

            for (int i = 0; i < fieldCount; i++)
            {
                // map OGR field to ArcObjects
                OSGeo.OGR.FieldDefn fieldDef = featDef.GetFieldDefn(i);

                ESRI.ArcGIS.Geodatabase.IFieldEdit2 fieldEdit = new ESRI.ArcGIS.Geodatabase.FieldClass();
                fieldEdit.Name_2 = fieldDef.GetName();
                fieldEdit.AliasName_2 = fieldDef.GetName();

                // map type
                OSGeo.OGR.FieldType ogrFieldType = fieldDef.GetFieldType();
                ESRI.ArcGIS.Geodatabase.esriFieldType mappedType;
                switch (ogrFieldType)
                {
                    case OSGeo.OGR.FieldType.OFTInteger: 
                        mappedType = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeInteger;
                        break;

                    case OSGeo.OGR.FieldType.OFTReal:
                        mappedType = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeDouble;
                        break;

                    case OSGeo.OGR.FieldType.OFTString:
                        mappedType = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeString;
                        break;

                    case OSGeo.OGR.FieldType.OFTBinary:
                        mappedType = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeBlob;
                        break;

                    case OSGeo.OGR.FieldType.OFTDateTime:
                        mappedType = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeDate;
                        break;

                    default:
                        mappedType = ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeString;
                        break;
                }


                fieldEdit.Type_2 = mappedType;

                outFieldMap.Add(fieldsInserted, i);

                fieldArray.Add(fieldEdit);

                fieldsInserted++;                                
            }

            // Add all the fields from the array to an ESRI fields class object. The reason that we do that
            // here is that we need to know the count in advance

            ESRI.ArcGIS.Geodatabase.IFieldsEdit fields = new ESRI.ArcGIS.Geodatabase.FieldsClass();
            fields.FieldCount_2 = fieldArray.Count;

            for (int i = 0; i < fieldArray.Count; i++)
            {
                fields.set_Field(i, fieldArray[i] as ESRI.ArcGIS.Geodatabase.IField);
            }

            outFields = fields;
        }

        public static ESRI.ArcGIS.Geometry.ISpatialReference get_spatialReference(OSGeo.OSR.SpatialReference ogrSR)
        {
            ogrSR.MorphToESRI();

            string wkt;
            ogrSR.ExportToWkt(out wkt);

            ISpatialReferenceFactory4 spatialReferenceFactory = new ESRI.ArcGIS.Geometry.SpatialReferenceEnvironmentClass();
            ISpatialReference sr;

            int bytesRead;
            spatialReferenceFactory.CreateESRISpatialReference(wkt, out sr, out bytesRead);

            return sr;
        }

        public static bool ogr_geo_type_to_esri_geo_type(OSGeo.OGR.wkbGeometryType ogrType,
                                                        out esriGeometryType outGDBType, out bool outHasZ)
        {

            switch (ogrType)
            {
                case OSGeo.OGR.wkbGeometryType.wkbPoint25D:
                    {
                        outGDBType = esriGeometryType.esriGeometryPoint;
                        outHasZ = true;
                        break;
                    }

                case OSGeo.OGR.wkbGeometryType.wkbMultiPoint25D:
                    {
                        outGDBType = esriGeometryType.esriGeometryMultipoint;
                        outHasZ = true;
                        break;
                    }

                case OSGeo.OGR.wkbGeometryType.wkbLineString25D:
                case OSGeo.OGR.wkbGeometryType.wkbMultiLineString25D:
                    {
                        outGDBType = esriGeometryType.esriGeometryPolyline;
                        outHasZ = true;
                        break;
                    }

                case OSGeo.OGR.wkbGeometryType.wkbPolygon25D:
                case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon25D:
                    {
                        outGDBType = esriGeometryType.esriGeometryPolygon;
                        outHasZ = true;
                        break;
                    }

                /* 2D forms */
                case OSGeo.OGR.wkbGeometryType.wkbPoint:
                    {
                        outGDBType = esriGeometryType.esriGeometryPoint;
                        outHasZ = false;
                        break;
                    }

                case OSGeo.OGR.wkbGeometryType.wkbMultiPoint:
                    {
                        outGDBType = esriGeometryType.esriGeometryMultipoint;
                        outHasZ = false;
                        break;
                    }

                case OSGeo.OGR.wkbGeometryType.wkbLineString:
                case OSGeo.OGR.wkbGeometryType.wkbMultiLineString:
                    {
                        outGDBType = esriGeometryType.esriGeometryPolyline;
                        outHasZ = false;
                        break;
                    }

                case OSGeo.OGR.wkbGeometryType.wkbPolygon:
                case OSGeo.OGR.wkbGeometryType.wkbMultiPolygon:
                    {
                        outGDBType = esriGeometryType.esriGeometryPolygon;
                        outHasZ = false;
                        break;
                    }

                default:
                    {
                        //CPLError( CE_Failure, CPLE_AppDefined, "Cannot map OGRwkbGeometryType (%s) to ESRI type",
                        //          OGRGeometryTypeToName(ogrType));

                        outGDBType = esriGeometryType.esriGeometryNull;
                        outHasZ = false;

                        return false;
                    }
            }
            return true;
        }

        // Until the OGR bindings handle UTF16 chars correctly we have to do this ugly workaround.
        // For background on the issue, see http://trac.osgeo.org/gdal/ticket/4971
        // The workaround involves grabbing the utf16 bytes and just picking even characters
        // and shoving them in byte array. Then treat those as utf8 bytes and convert them to utf16

        public static string ghetto_fix_ogr_string(string ogr_fake_utf16)
        {
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(ogr_fake_utf16);
            int byteCount = utf16Bytes.Length;
            byte[] utf8Bytes = new byte[byteCount / 2];

            for (int i=0, j=0; i < byteCount / 2; i++, j+=2)
                utf8Bytes[i] = utf16Bytes[j];

            return Encoding.UTF8.GetString(utf8Bytes,0, utf8Bytes.Length);
        }

    }
}
