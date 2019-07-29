using System;
using System.Data;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aipuer.Common
{
    public class Export
    {
        public Export()
        {
        }

        public string exportExcel(string fileName, string serverUrl, string serverFlag, string serverConnName, string layer, string wkt = null, string where = null, string whereKey = null, string whereValue = null, string statistics = null, string clip = null)
        {
            string daochu_result = "";
            {
                try
                {

                    string fileNameDir = System.AppDomain.CurrentDomain.BaseDirectory;
                    string saveFilePath = fileNameDir + "\\resource\\download\\excel";
                    if (!Directory.Exists(saveFilePath))
                    {
                        Directory.CreateDirectory(saveFilePath);
                    }
                    {
                        XSSFWorkbook workBook = new XSSFWorkbook();

                        XSSFFont fieldFont = (XSSFFont)workBook.CreateFont();
                        fieldFont.FontHeightInPoints = 13;
                        fieldFont.Boldweight = 600;
                        ICellStyle fieldStyle = workBook.CreateCellStyle();
                        fieldStyle.Alignment = HorizontalAlignment.Center;
                        fieldStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                        fieldStyle.SetFont(fieldFont);
                        ////下面这两行颜色不管用
                        fieldStyle.FillForegroundColor = IndexedColors.Blue.Index;
                        fieldStyle.FillBackgroundColor = IndexedColors.Red.Index;


                        ICellStyle dataStyle = workBook.CreateCellStyle();
                        dataStyle.Alignment = HorizontalAlignment.Center;
                        dataStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中

                        
                        {
                            string returnstr = "";
                            Common common = new Common();
                            
                            XSSFSheet sheet_sub = (XSSFSheet)workBook.CreateSheet(layer);
                            if (serverFlag == "query")
                            {
                                IRow _frow = sheet_sub.CreateRow(0);
                                _frow.Height = 3 * 256;//行高                            
                                _frow.CreateCell(0).SetCellValue("序号");
                                _frow.GetCell(0).CellStyle = fieldStyle;

                                string url = serverUrl + "query?serverConnName=" + serverConnName + "&tableName=" + layer + "&wkt=" + wkt + "&where=" + where + "&clip=" + clip + "&whereValue=" + whereValue + "&whereKey=" + whereKey;
                                returnstr = common.queryDataPost(url);
                                if (returnstr.IndexOf("\"result\":\"error\"") > -1)
                                {
                                    daochu_result = "\"exportResult\":\"error\",\"exportMessage\":\"error\"";
                                    workBook.Close();
                                }
                                var jo = (JObject)JsonConvert.DeserializeObject(returnstr);
                                if (jo["result"].ToString() == "success")
                                {
                                    var joo = (JObject)jo["data"];
                                    var ja = (JArray)joo["features"];
                                    var index = 0;
                                    foreach (JObject items in ja)
                                    {
                                        IRow _frowData = sheet_sub.CreateRow(index + 1);
                                        _frowData.Height = 2 * 256;//行高
                                        _frowData.CreateCell(0).SetCellValue(index + 1);
                                        _frowData.GetCell(0).CellStyle = dataStyle;

                                        JObject _properties = (JObject)items["properties"];
                                        var _colIndex = 1;
                                        foreach (var _item in _properties)
                                        {
                                            var _key = _item.Key;
                                            var _value = _item.Value;
                                            if (_key.ToLower() == "gid" || _key.ToLower() == "calarea")
                                            {
                                                continue;
                                            }
                                            if(index == 0)
                                            {
                                                //填充表头
                                                _frow.CreateCell(_colIndex).SetCellValue(_key);
                                                _frow.GetCell(_colIndex).CellStyle = fieldStyle;
                                                sheet_sub.SetColumnWidth(_colIndex, 18 * 256);
                                            }
                                            _frowData.CreateCell(_colIndex).SetCellValue(_value.ToString());
                                            _frowData.GetCell(_colIndex).CellStyle = dataStyle;
                                            _colIndex++;
                                        }
                                        index++;
                                    }
                                }
                            }
                            else if (serverFlag == "statistics")
                            {
                                IRow _frow = sheet_sub.CreateRow(0);
                                _frow.Height = 3 * 256;//行高                            
                                _frow.CreateCell(0).SetCellValue("序号");
                                _frow.CreateCell(1).SetCellValue("类型");
                                _frow.CreateCell(2).SetCellValue("数量");
                                _frow.CreateCell(3).SetCellValue("面积(亩)");

                                string url = serverUrl + "statistics?serverConnName=" + serverConnName + "&tableName=" + layer + "&wkt=" + wkt + "&where=" + where + "&clip=" + clip + "&whereValue=" + whereValue + "&whereKey=" + whereKey + "&fieldName=" + statistics;
                                returnstr = common.queryDataPost(url);
                                if (returnstr.IndexOf("\"result\":\"error\"") > -1)
                                {
                                    daochu_result = "\"exportResult\":\"error\",\"exportMessage\":\"error\"";
                                    workBook.Close();
                                }
                                var jo = (JObject)JsonConvert.DeserializeObject(returnstr);
                                if (jo["result"].ToString() == "success")
                                {
                                    var ja = (JArray)jo["data"];
                                    var index = 0;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        _frow.GetCell(i).CellStyle = fieldStyle;
                                        sheet_sub.SetColumnWidth(i, 18 * 256);
                                    }
                                    foreach (JObject items in ja)
                                    {
                                        string _name = "", _amount = "", _calarea = "";
                                        foreach (var item in items)
                                        {
                                            switch (item.Key)
                                            {
                                                case "name":
                                                    _name = item.Value.ToString();
                                                    break;
                                                case "amount":
                                                    _amount = item.Value.ToString();
                                                    break;
                                                case "calarea":
                                                    _calarea = item.Value.ToString();
                                                    break;
                                            }
                                        }
                                        IRow _frowData = sheet_sub.CreateRow(index + 1);
                                        _frowData.Height = 2 * 256;//行高
                                        _frowData.CreateCell(0).SetCellValue(index + 1);
                                        _frowData.CreateCell(1).SetCellValue(_name);
                                        _frowData.CreateCell(2).SetCellValue(_amount);
                                        string _mj = areaTransfer(_calarea);
                                        _frowData.CreateCell(3).SetCellValue(_mj);
                                        for (int jj = 0; jj < 4; jj++)
                                        {
                                            _frowData.GetCell(jj).CellStyle = dataStyle;
                                        }
                                        index++;
                                    }
                                }
                            }
                        }
                        //保存
                        try
                        {
                            string saveFileName = saveFilePath + "\\" + fileName + ".xlsx";
                            using (FileStream fs = new FileStream(saveFileName, FileMode.Create))
                            {
                                workBook.Write(fs);
                                workBook.Close();
                                daochu_result = "\"exportResult\":\"success\",\"exportMessage\":\"" + "/resource/download/excel/" + fileName + ".xlsx" + "\"";
                            }
                        }
                        catch (System.Exception e)
                        {
                            daochu_result = "\"exportResult\":\"error\",\"exportMessage\":\"" + e.ToString() + "\"";
                            workBook.Close();
                        }
                    }

                }
                catch (Exception e)
                {
                    daochu_result = "\"exportResult\":\"errpr\",\"exportMessage\":\"" + e.ToString() + "\"";
                }
            }
            return daochu_result;
        }

        public string screenshotExportExcel(string fileName, string imgPath, string wkt = null, string serverUrl = null, string serverConnName = null, string layer = null, string layerText = null, string statistics = null, string statisticsText = null)
        {
            string daochu_result = "";
            {
                try
                {

                    string fileNameDir = System.AppDomain.CurrentDomain.BaseDirectory;
                    string saveFilePath = fileNameDir + "\\resource\\download\\excel";
                    if (!Directory.Exists(saveFilePath))
                    {
                        Directory.CreateDirectory(saveFilePath);
                    }
                    {
                        XSSFWorkbook workBook = new XSSFWorkbook();

                        XSSFFont fieldFont = (XSSFFont)workBook.CreateFont();
                        fieldFont.FontHeightInPoints = 13;
                        fieldFont.Boldweight = 600;
                        ICellStyle fieldStyle = workBook.CreateCellStyle();
                        fieldStyle.Alignment = HorizontalAlignment.Center;
                        fieldStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                        fieldStyle.SetFont(fieldFont);
                        ////下面这两行颜色不管用
                        fieldStyle.FillForegroundColor = IndexedColors.Blue.Index;
                        fieldStyle.FillBackgroundColor = IndexedColors.Red.Index;
                        XSSFSheet sheet = (XSSFSheet)workBook.CreateSheet("截图");
                        sheet.SetColumnWidth(0, 50 * 256);
                        if (string.IsNullOrEmpty(wkt))
                        {
                            //只保存图片
                            IRow _firstRow = sheet.CreateRow(0);
                            ICell _firstCell = _firstRow.CreateCell(0);
                            _firstCell.SetCellValue(DateTime.Now.ToString("yyyy-MM-dd"));
                            _firstCell.CellStyle = fieldStyle;

                            IRow _picRow = sheet.CreateRow(1);
                            _picRow.Height = 70 * 256;//行高，存放地图

                            // 画图的顶级管理器，一个sheet只能获取一个（一定要注意这点）
                            XSSFDrawing patriarch = (XSSFDrawing)sheet.CreateDrawingPatriarch();
                            //在rowindex=1行col=0列插入图片，图片占1行1列
                            setPic(workBook, patriarch, fileNameDir + "/" + imgPath, sheet, 1, 0, 1, 1);
                        }
                        else
                        {
                            //如果A4导出，则不要截图                            
                            workBook.RemoveAt(0);
                        }
                        if (!string.IsNullOrEmpty(wkt))
                        {
                            string returnstr = "";
                            Common common = new Common();

                            string wktArea = "0";
                            string url = serverUrl + "area?serverConnName=" + serverConnName + "&wkt=" + wkt;
                            returnstr = common.queryDataPost(url);
                            JObject jo = (JObject)JsonConvert.DeserializeObject(returnstr);
                            if (jo["result"].ToString() == "success")
                            {
                                wktArea = ((JArray)jo["data"])[0]["area1"].ToString();
                                wktArea = areaTransfer(wktArea);
                            }

                            ICellStyle dataStyle = workBook.CreateCellStyle();
                            dataStyle.Alignment = HorizontalAlignment.Center;
                            dataStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中



                            returnstr = common.queryDataPost(url);
                            if (!string.IsNullOrEmpty(layer) && !string.IsNullOrEmpty(layerText) && !string.IsNullOrEmpty(statistics) && !string.IsNullOrEmpty(statisticsText))
                            {
                                var layerArray = layer.Split(',');
                                var layerTextArray = layerText.Split(',');
                                var statisticsArray = statistics.Split(',');
                                var statisticsTextArray = statisticsText.Split(',');
                                if (layerArray.Length == layerTextArray.Length && statisticsArray.Length == statisticsTextArray.Length && layerTextArray.Length == statisticsArray.Length)
                                {
                                    for (var j = 0; j < layerArray.Length; j++)
                                    {
                                        string tableName = layerArray[j];
                                        string statisticsField = statisticsArray[j];
                                        XSSFSheet sheet_sub = (XSSFSheet)workBook.CreateSheet(layerTextArray[j]);
                                        IRow _frow = sheet_sub.CreateRow(0);
                                        _frow.Height = 3 * 256;//行高                            
                                        _frow.CreateCell(0).SetCellValue("序号");
                                        _frow.CreateCell(1).SetCellValue("总面积(亩)");
                                        _frow.CreateCell(2).SetCellValue(statisticsTextArray[j]);
                                        _frow.CreateCell(3).SetCellValue("面积(亩)");

                                        for (int i = 0; i < 4; i++)
                                        {
                                            _frow.GetCell(i).CellStyle = fieldStyle;
                                            sheet_sub.SetColumnWidth(i, 18 * 256);
                                        }

                                        url = serverUrl + "statistics?serverConnName=" + serverConnName + "&tableName=" + tableName + "&wkt=" + wkt + "&clip=true&fieldName=" + Microsoft.JScript.GlobalObject.escape(statisticsField);
                                        returnstr = common.queryDataPost(url);
                                        if (returnstr.IndexOf("\"result\":\"error\"") > -1)
                                        {
                                            continue;
                                        }
                                        jo = (JObject)JsonConvert.DeserializeObject(returnstr);
                                        if (jo["result"].ToString() == "success")
                                        {
                                            var ja = (JArray)jo["data"];
                                            var index = 0;
                                            foreach (JObject items in ja)
                                            {
                                                string _name = "", _amount = "", _cliparea = "", _calarea = "";
                                                foreach (var item in items)
                                                {
                                                    switch (item.Key)
                                                    {
                                                        case "name":
                                                            _name = item.Value.ToString();
                                                            break;
                                                        case "amount":
                                                            _amount = item.Value.ToString();
                                                            break;
                                                        case "cliparea":
                                                            _cliparea = item.Value.ToString();
                                                            break;
                                                        case "calarea":
                                                            _calarea = item.Value.ToString();
                                                            break;
                                                    }
                                                }
                                                IRow _frowData = sheet_sub.CreateRow(index + 1);
                                                _frowData.Height = 2 * 256;//行高
                                                _frowData.CreateCell(0).SetCellValue(index + 1);
                                                if (index == 0)
                                                {
                                                    _frowData.CreateCell(1).SetCellValue(wktArea);
                                                }
                                                else
                                                {
                                                    _frowData.CreateCell(1).SetCellValue("");
                                                }
                                                _frowData.CreateCell(2).SetCellValue(_name);
                                                string _mj = areaTransfer(_cliparea);
                                                _frowData.CreateCell(3).SetCellValue(_mj);
                                                for (int jj = 0; jj < 4; jj++)
                                                {
                                                    _frowData.GetCell(jj).CellStyle = dataStyle;
                                                }
                                                index++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //保存
                        try
                        {
                            string saveFileName = saveFilePath + "\\" + fileName + ".xlsx";
                            using (FileStream fs = new FileStream(saveFileName, FileMode.Create))
                            {
                                workBook.Write(fs);
                                workBook.Close();
                                daochu_result = "\"exportResult\":\"success\",\"exportMessage\":\"" + "/resource/download/excel/" + fileName + ".xlsx" + "\"";
                            }
                        }
                        catch (System.Exception e)
                        {
                            daochu_result = "\"exportResult\":\"error\",\"exportMessage\":\"" + e.ToString() + "\"";
                            workBook.Close();
                        }
                    }

                }
                catch (Exception e)
                {
                    daochu_result = "\"exportResult\":\"errpr\",\"exportMessage\":\"" + e.ToString() + "\"";
                }
            }
            return daochu_result;
        }

        public string singleExportExcel(string fileName, string data)
        {
            string daochu_result = "";
            {
                try
                {
                    string fileNameDir = System.AppDomain.CurrentDomain.BaseDirectory;
                    string saveFilePath = fileNameDir + "\\resource\\download\\excel";
                    if (!Directory.Exists(saveFilePath))
                    {
                        Directory.CreateDirectory(saveFilePath);
                    }
                    {
                        XSSFWorkbook workBook = new XSSFWorkbook();

                        XSSFFont fieldFont = (XSSFFont)workBook.CreateFont();
                        fieldFont.FontHeightInPoints = 13;
                        fieldFont.Boldweight = 600;
                        ICellStyle fieldStyle = workBook.CreateCellStyle();
                        fieldStyle.Alignment = HorizontalAlignment.Center;
                        fieldStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                        fieldStyle.SetFont(fieldFont);
                        ////下面这两行颜色不管用
                        fieldStyle.FillForegroundColor = IndexedColors.Blue.Index;
                        fieldStyle.FillBackgroundColor = IndexedColors.Red.Index;
                        
                        ICellStyle dataStyle = workBook.CreateCellStyle();
                        dataStyle.Alignment = HorizontalAlignment.Center;
                        dataStyle.VerticalAlignment = VerticalAlignment.Center;//垂直居中
                        
                        {
                            string returnstr = "";
                            Common common = new Common();

                            returnstr = data;
                            {
                                var jo = (JObject)JsonConvert.DeserializeObject(returnstr);
                                {
                                    //填充基础信息
                                    XSSFSheet sheet_sub = (XSSFSheet)workBook.CreateSheet("基础信息");
                                    IRow _frow = sheet_sub.CreateRow(0);
                                    _frow.Height = 3 * 256;//行高                            
                                    _frow.CreateCell(0).SetCellValue("序号");
                                    _frow.CreateCell(1).SetCellValue("类型");
                                    _frow.CreateCell(2).SetCellValue("内容");
                                    for (var i = 0; i < 3; i++)
                                    {
                                        _frow.GetCell(i).CellStyle = fieldStyle;
                                        if(i == 2)
                                        {
                                            sheet_sub.SetColumnWidth(i, 23 * 256);
                                        }
                                        else
                                        {
                                            sheet_sub.SetColumnWidth(i, 18 * 256);
                                        }
                                    }
                                    JObject _properties = (JObject)jo["基础信息"];
                                    var index = 0;
                                    foreach (var _item in _properties)
                                    {
                                        IRow _frowData = sheet_sub.CreateRow(index + 1);
                                        _frowData.Height = 2 * 256;//行高
                                        _frowData.CreateCell(0).SetCellValue(index + 1);

                                        var _key = _item.Key;
                                        var _value = _item.Value;
                                        
                                        _frowData.CreateCell(1).SetCellValue(_key.ToString());
                                        _frowData.CreateCell(2).SetCellValue(_value.ToString());

                                        for (var j = 0; j < 3; j++)
                                        {
                                            _frowData.GetCell(j).CellStyle = dataStyle;
                                        }
                                        index++;
                                    }
                                }
                                {
                                    var ja = (JArray)jo["地块信息"];
                                    XSSFSheet sheet_sub = (XSSFSheet)workBook.CreateSheet("地块信息");
                                    IRow _frow = sheet_sub.CreateRow(0);
                                    _frow.Height = 3 * 256;//行高                            
                                    _frow.CreateCell(0).SetCellValue("序号");
                                    _frow.CreateCell(1).SetCellValue("类型");
                                    _frow.CreateCell(2).SetCellValue("数量");
                                    _frow.CreateCell(3).SetCellValue("面积(亩)");
                                    for (var i = 0; i < 4; i++)
                                    {
                                        _frow.GetCell(i).CellStyle = fieldStyle;
                                        sheet_sub.SetColumnWidth(i, 18 * 256);
                                    }
                                    var index = 0;
                                    foreach (JObject items in ja)
                                    {
                                        IRow _frowData = sheet_sub.CreateRow(index + 1);
                                        _frowData.Height = 2 * 256;//行高
                                        _frowData.CreateCell(0).SetCellValue(index + 1);
                                        _frowData.GetCell(0).CellStyle = dataStyle;

                                        foreach (var _item in items)
                                        {
                                            var _key = _item.Key;
                                            var _value = _item.Value;
                                            var _colIndex = -1;
                                            switch (_key)
                                            {
                                                case "name":
                                                    _colIndex = 1;
                                                    break;
                                                case "amount":
                                                    _colIndex = 2;
                                                    break;
                                                case "area":
                                                    _colIndex = 3;
                                                    break;
                                            }
                                            _frowData.CreateCell(_colIndex).SetCellValue(_value.ToString());
                                            _frowData.GetCell(_colIndex).CellStyle = dataStyle;
                                        }
                                        index++;
                                    }
                                }
                            }
                        }
                        //保存
                        try
                        {
                            string saveFileName = saveFilePath + "\\" + fileName + ".xlsx";
                            using (FileStream fs = new FileStream(saveFileName, FileMode.Create))
                            {
                                workBook.Write(fs);
                                workBook.Close();
                                daochu_result = "\"exportResult\":\"success\",\"exportMessage\":\"" + "/resource/download/excel/" + fileName + ".xlsx" + "\"";
                            }
                        }
                        catch (System.Exception e)
                        {
                            daochu_result = "\"exportResult\":\"error\",\"exportMessage\":\"" + e.ToString() + "\"";
                            workBook.Close();
                        }
                    }

                }
                catch (Exception e)
                {
                    daochu_result = "\"exportResult\":\"errpr\",\"exportMessage\":\"" + e.ToString() + "\"";
                }
            }
            return daochu_result;
        }

        private string areaTransfer(string source)
        {
            try
            {
                double area = double.Parse(source);
                area = Math.Round(area / 666.6666667, 2);
                source = area.ToString();
            }
            catch
            {
                source = "error";
            }
            return source;
        }


        private void setPic(XSSFWorkbook workbook, XSSFDrawing patriarch, string path, ISheet sheet, int rowline, int col, int rowMergerPlus = 0, int colMergerPlus = 0)
        {
            if (string.IsNullOrEmpty(path)) return;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            int pictureIdx = workbook.AddPicture(bytes, PictureType.JPEG);
            // 插图片的位置  HSSFClientAnchor（dx1,dy1,dx2,dy2,col1,row1,col2,row2) 后面再作解释
            XSSFClientAnchor anchor = new XSSFClientAnchor(0, 0, 0, 0, col, rowline, col + colMergerPlus, rowline + rowMergerPlus);
            //把图片插到相应的位置
            XSSFPicture pict = (XSSFPicture)patriarch.CreatePicture(anchor, pictureIdx);
        }

    }
}
