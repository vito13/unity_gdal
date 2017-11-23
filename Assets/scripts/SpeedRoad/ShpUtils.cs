using OSGeo.OGR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShpUtils {
    static public bool SaveShp(string path, string fanme, wkbGeometryType t, List<Geometry> lstgeo)
    {
        // 为了支持中文路径，请添加下面这句代码  
        OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
        // 为了使属性表字段支持中文，请添加下面这句  
        OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");

        string strVectorFile = path;

        // 注册所有的驱动  
        Ogr.RegisterAll();

        //创建数据，这里以创建ESRI的shp文件为例  
        string strDriverName = "ESRI Shapefile";
        Driver oDriver = Ogr.GetDriverByName(strDriverName);
        if (oDriver == null)
        {
            // Console.WriteLine("%s 驱动不可用！\n", strVectorFile);
            return false;
        }

        // 创建数据源  
        DataSource oDS = oDriver.CreateDataSource(strVectorFile, null);
        if (oDS == null)
        {
            // Console.WriteLine("创建矢量文件【%s】失败！\n", strVectorFile);
            return false;
        }

        // 创建图层，创建一个多边形图层，这里没有指定空间参考，如果需要的话，需要在这里进行指定  
        Layer oLayer = oDS.CreateLayer(fanme, null, t, null);
        if (oLayer == null)
        {
            // Console.WriteLine("图层创建失败！\n");
            return false;
        }

        FeatureDefn oDefn = oLayer.GetLayerDefn();

        // 创建三角形要素  
        foreach (var item in lstgeo)
        {
            Feature oFeatureTriangle = new Feature(oDefn);
            oFeatureTriangle.SetGeometry(item);
            oLayer.CreateFeature(oFeatureTriangle);
        }
        
        return true;
    }

}
