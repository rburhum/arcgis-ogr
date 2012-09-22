# <a href="http://www.amigocloud.com">AmigoCloud</a>'s GDAL/OGR Plugin for ArcGIS
This repository contains the source code for the GDAL/OGR Plugin which is part of the <a href="http://www.amigocloud.com">AmigoCloud Mobile Data Collection Suite</a>

## Purpose
The GDAL/OGR Plugin adds read support to any of the <a href="http://www.gdal.org/ogr/ogr_formats.html">vector formats supported by GDAL/OGR.</a>

Some popular formats include:
* <a href="http://www.sqlite.org/">SQLite</a> / <a href="https://www.gaia-gis.it/fossil/libspatialite/index">Spatialite</a>
* NoSQL <a href="http://couchdb.apache.org/">CouchDB</a> / <a href="https://github.com/couchbase/geocouch">GeoCouch </a>
* <a href="https://sites.google.com/site/fusiontablestalks/stories">Google Fusion Tables<a/>
* AutoCAD <a href="http://www.gdal.org/ogr/drv_dwg.html">DWG</a> and <a href="http://www.gdal.org/ogr/drv_dxf.html">DXF</a>
* <a href="http://www.gdal.org/ogr/drv_gpx.html">GPS Exchange Format (GPX)</a>
* <a href="http://www.openstreetmap.org/">OpenStreetMap</a> <a href="http://www.gdal.org/ogr/drv_osm.html">XML and PBF</a>
* <a href="http://www.gdal.org/ogr/drv_mitab.html">MapInfo TAB and MIF/MID</a>
* <a href="http://www.gdal.org/ogr/drv_mysql.html">MySQL</a>
* <a href="http://www.nauticalcharts.noaa.gov/mcd/enc/">S57 Electronic Navigational Charts (ENC)</a>
* <a href="http://www.gdal.org/ogr/drv_xls.html">Microsoft Office Excel Format</a>
* ...and <a href="http://www.gdal.org/ogr/ogr_formats.html">much much more!</a>

## Screenshots

* OGR Add data dialog
<img src="http://i.imgur.com/Tc0tp.png">

* Spatialite and S57 files being read natively from ArcGIS
<img src="http://i.imgur.com/Svzjp.png" />
<img src="http://i.imgur.com/O2kaJ.png" />


## License
BSD License. For those of you not in Open Source geekdom, it means "hella free". You can pretty much, do whatever you want with it - Commercial or not.
We ask that you please contribute any modifications back if you are kind enough to make modifications, but you are not forced to.

## Download / Ready to use Binaries
Sorry, not available yet but come back very soon :)

## User Information (for Developer information see below)

### User FAQ
* My spatialite is added to ArcMap as a standlone (i.e non-spatial table)
  Make sure your sqlite files are spatialite files. For this, there needs to be some <a href="http://www.gaia-gis.it/gaia-sins/spatialite-cookbook/html/metadata.html">metadata tables</a>. Don't add them manually though. Use the <a href="http://www.gaia-gis.it/gaia-sins/spatialite-manual-2.3.1.html">init_spatialite.sql file</a> instead. 
  Once you are done with this, the last thing to do is to make sure that the tables are registered with the geometry_columns table. If you are using the <a href="http://www.gaia-gis.it/spatialite-2.3.1/binaries.html">Spatialite GUI</a>, this is as easy as right clicking on the geometry column and chooseing the *Recover Geometry* option. If you are doing it through the command prompt, you will find that the <a href="http://www.gaia-gis.it/spatialite-2.3.1/spatialite-tutorial-2.3.1.html">RecoverGeometryColumn</a> is what you are looking for.

## Developer Information

### Compiling
This project uses the <a href="http://trac.osgeo.org/gdal/wiki/GdalOgrInCsharp">GDAL CSharp Bindings</a> and of course <a href="http://www.gdal.org/">GDAL</a>, so you will
need to get binaries for those. If you don't want to go through the trouble of compiling GDAL from scratch, you should try the <a href="http://vbkto.dyndns.org/sdk/">GDAL SDK binaries compiled by Tamas Szekeres</a>
which I used to write the initial version of this Plugin. **Remember that you need the 32 bit version** because for some God-forsaken reason, ArcMap is still compiled for a 32-bit architecture even in 2012. At the time of this writing,
I used <a href="http://vbkto.dyndns.org/sdk/PackageList.aspx?file=release-1600-gdal-1-9-1-mapserver-6-0-3.zip">release-1600-gdal-1-9-1-mapserver-6-0-3.zip</a>. By the time you read this, a much newer version must exist.

### Developer FAQ
* My dll gets loaded but my breakpoints are not being hit. Help?
  Ah yes, I ran into this problem, too. Turns out that if you compile against the 4.0.NET Runtime, you will need to make a slight change to the ArcMap.exe.config file that is shipped with ArcGIS. <a href="http://resources.arcgis.com/en/help/arcobjects-net/conceptualhelp/index.html#/How_to_debug_add_ins/0001000002vs000000/">This little detail
  is described in their documentation.</a> After changing the supportedRuntime section, your breakpoints will be hit.
  
* Why is this implemented as a WorkspacePlugin? Doesn't it give you read support only?
  I started writing everything on C++ implementing every single COM interface that the GeoDatabase uses so that I would fool ArcMap/ArcCatalog into
  thinking I was a full-blown GeoDatabase. I had it somewhat working, but there was always some interface that I had missed (and was not documented properly).
  After hearing directly from the source, that in the next ArcGIS release, WorkspacePlugin's will have write support, I thrashed the other project that was taking
  weeks to this which only took two days. I don't regret it a bit.

* Does this run on ArcGIS Engine? Or Does it need Desktop?
  Both the ICommands and the WorkspacePlugin code live in the same assembly, so ArcGIS Desktop is required (ArcView / Basic / or whatever the cheapest license is called now should work). 
  There is nothing that is stopping you from separating the commands out and it should be very easy to do. I did not because I did not have an ArcGIS Engine license to test it so why do the work?
  
* Why did you check-in the <a href="https://github.com/RBURHUM/arcgis-ogr/blob/master/src/OGRPlugin/OGRPlugin/OGRPlugin.csproj.user">.csproj.user</a> file you nimrod!?!? Don't you know you are supposed to leave those out?
  Turns out that I am using Visual Studio Express 2010 to compile and debug this thing. <a href="http://through-the-interface.typepad.com/through_the_interface/2006/07/debugging_using.html">I had to hand edit the proj and 
  the user file to allow firing up ArcMap and have Release/Debug options in my IDE</a> since those options are available on the Express editions of Visual Studio. It seems that is what the ArcGIS Visual Studio Templates also do.
  
## Known Limitations

* Blob fields are not being read
  This is a limitation with the current Swig bindings for OGR. <a href="http://trac.osgeo.org/gdal/ticket/4457">It is being worked on. See the related ticket 4457</a>
* Workspace is readonly
  In <a href="http://downloads.esri.com/support/downloads/other_/189810.1_SP1_Announcement.pdf">ArcGIS 10.1 SP1 (October 2012)</a> ESRI will release a few interfaces that will allow write support to be easily implemented. Let's wait for them :)

## Contributing

Please fork this repository and contribute back using
[pull requests](https://github.com/RBURHUM/arcgis-ogr/pulls).

Any contributions, large or small, major features, bug fixes, additional
language translations, unit/integration tests are welcomed and appreciated
but will be thoroughly reviewed and discussed. And of course it would make you cool. You want to be cool right?