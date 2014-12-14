using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace CommonClassesLib.CommonClasses
{
    public static class ProjectCommonData
    {
        /// <summary>
        /// список соответсвия идентификаторов тегов и имен типов для уровня Native
        /// </summary>
        public static SortedList<string, string> slGlobalListTagsTypeNativeLevel = new SortedList<string, string>();
        /// <summary>
        /// список соответсвия идентификаторов тегов и имен типов для уровня PL (Discret, Analog, ...)
        /// </summary>
        public static SortedList<string, string> slGlobalListTagsType_PL = new SortedList<string, string>();
        /// <summary>
        ///	Признак качества переменной 
        /// </summary> 
        public enum VarQuality
        {
            vqUndefined = 0,        // Не определено (не производилось ни одного чтения, нет связи)
            vqGood = 1,             // Хорошее качество
            vqArhiv = 2,            // архивная переменная (из БД)
            vqRangeError = 3,       // Выход за пределы диапазона
            vqHandled = 4,          // Ручной ввод данных
            vqUknownTag = 5,        // несуществующий тег (? что значит не существующий тег - м.б. это может исп. в ответах на запросы когда запрашивается тег кот. нет, тогда возвращ его ид и это знач качества)
            vqErrorConverted = 6,   // ошибка преобразования в целевой тип
            vqNonExistDevice = 7,   // несуществующее устройство
            vqTagLengthIs0 = 8,      // длина запрашиваемого тега нулевая
            vqUknownError = 9,       // неизвестная ошибка при попытке получения значения тега
            /*
             * тег неактуален из-за
             * нарушения связи между
             * Dsr и Ds 
             * (это качество формируется на роутере)
             */
            vqDsr2DsBadConnection = 10,
            /*
             * ручной вычисляемый тег -
             * это качество устанавливается 
             * для расчетного тега если все составляющие его теги имеют хорошее качество
             * а хотя бы один (или все) - ручное
             */
            vqCalculatedHanle = 11
        }
        /// <summary>
        /// получить путь к файлу Project.cfg
        /// </summary>
        /// <returns></returns>
        public static string GetPathToPrjFile()
        {
            string PathToPrjFile = string.Empty;
            try
            {
                PathToPrjFile = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Project" + Path.DirectorySeparatorChar + "Project.cfg";
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return PathToPrjFile;
        }
        /// <summary>
        /// получитть путь к файлу Configuration.cfg
        /// </summary>
        /// <returns></returns>
        public static string GetPathToConfigurationFile()
        {
            string PathToConfigurationFile  = string.Empty;

            try
            {
                PathToConfigurationFile = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "Project" + Path.DirectorySeparatorChar + "Configuration.cfg";
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return PathToConfigurationFile;
        }
        /// <summary>
        /// получить путь к папке с файлом PrgDevCFG.cdp
        /// по имени источника
        /// </summary>
        /// <param name="src_name"></param>
        /// <returns></returns>
        public static string GetPathTo_PrgDevCFG_cdp_File(string src_name)
        {
            string PathTo_PrgDevCFG_cdp_File = string.Empty;

            try
            {
                PathTo_PrgDevCFG_cdp_File = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Project", "Configuration", "Sources", src_name, "PrgDevCFG.cdp");
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return PathTo_PrgDevCFG_cdp_File;
        }
        /// <summary>
        /// получить путь к файлу 
        /// с описанием устройства в папке \Devices
        /// </summary>
        /// <param name="numdev">guid устройства</param>
        /// <returns></returns>
        public static string GetPathTo_DevCFG_File(uint numdev)
        {
            string PathTo_DevCFG_File = string.Empty;

            try
            {
                /*
                 * перечислить файлы в папке \Devices
                 * и найти нужный файл по номеру в первой части 
                 * названия файла nn@тип файла
                 */

                string namefolder = Path.Combine(Path.GetDirectoryName(GetPathToPrjFile()), "Configuration", "Devices");
                List<string> lstCfgFiles = new List<string>();

                DirectoryInfo di = new DirectoryInfo(namefolder);
                foreach (FileInfo fi in di.GetFiles("*.cfg"))
                    lstCfgFiles.Add(fi.FullName);

                foreach (string fn in lstCfgFiles)
                {
                    string[] fns = Path.GetFileName(fn).Split(new char[] { '@' });

                    if (fns[0] == numdev.ToString())
                    {
                        PathTo_DevCFG_File = Path.Combine(fn);
                        break;
                    }
                }                
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return PathTo_DevCFG_File;
        }
        #region TagValue для сравнения
        /// <summary>
        /// последнее сформированное
        /// значение тега - требуется для 
        /// точек кода где сравниваются 
        /// новое и предыдущее значение тега
        /// </summary>
        public class DSTagValueCompare
        {
            /// <summary>
            /// качество тега
            /// </summary>
            public VarQuality VarQuality
            {
                get { return varquality; }
                set { varquality = value; }
            }
            VarQuality varquality = VarQuality.vqUndefined;

            /// <summary>
            /// значение тега в Object
            /// </summary>
            public object VarValueAsObject
            {
                get { return varvalueasobject; }
                set { varvalueasobject = value; }
            }
            object varvalueasobject = null;

            public DSTagValueCompare()
            {
                try
                {
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                }
            }

            public DSTagValueCompare(VarQuality VarQuality, object varvalue)
            {
                try
                {
                    varquality = VarQuality;
                    varvalueasobject = varvalue;
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                }
            }
        }
        /// <summary>
        /// сравнить значения тега как objects
        /// c учетом изменения качества
        /// </summary>
        /// <param name="dstvcOld"></param>
        /// <param name="dstvNew"></param>
        /// <returns>true - значения равны, false - не равны</returns>
        public static bool IsTagAsObjectsIsEqual(DSTagValueCompare dstvcOld, DSTagValueCompare dstvNew)
        {
            bool rez = true;    // по умолячанию значения равны
            bool rezq = true;    // по умолячанию качества равны
            try
            {
                if (dstvcOld.VarValueAsObject == null)
                {
                    if (dstvNew.VarValueAsObject == null)
                        return true;
                    else
                        return false;
                }
                if (dstvNew.VarValueAsObject == null)
                {
                    if (dstvcOld.VarValueAsObject == null)
                        return true;
                    else
                        return false;
                }

                string type4OldValue = dstvcOld.VarValueAsObject.GetType().ToString();
                string type4NewValue = dstvNew.VarValueAsObject.GetType().ToString();
                if (type4OldValue != type4NewValue)
                {
                    if (type4OldValue == "System.String" || type4NewValue == "System.String")
                    {
                        string stold = Convert.ToString(dstvcOld.VarValueAsObject);
                        string stnew = Convert.ToString(dstvNew.VarValueAsObject);
                        rez = stold == stnew ? true : false;
                    }
                    else if (type4OldValue == "System.Double" || type4NewValue == "System.Int32")
                    {
                        Int32 i32old = Convert.ToInt32(dstvcOld.VarValueAsObject);
                        Int32 i32new = Convert.ToInt32(dstvNew.VarValueAsObject);
                        rez = i32old == i32new ? true : false;
                    }
                    else
                        throw new Exception(string.Format(@"(242) ...\00_DataServer\CommonClasses\CommonUtils.cs: CompareTagAsObjects() : Несовпадение типов (type4OldValue = {0} != type4NewValue = {1}) .", type4OldValue, type4NewValue));
                }
                else
                {
                    switch (type4OldValue)
                    {
                        case "System.Byte":
                            Byte Byteold = Convert.ToByte(dstvcOld.VarValueAsObject);
                            Byte Bytenew = Convert.ToByte(dstvNew.VarValueAsObject);
                            rez = Byteold == Bytenew ? true : false;
                            break;
                        case "System.SByte":
                            SByte SByteold = Convert.ToSByte(dstvcOld.VarValueAsObject);
                            SByte SBytenew = Convert.ToSByte(dstvNew.VarValueAsObject);
                            rez = SByteold == SBytenew ? true : false;
                            break;
                        case "System.Boolean":
                            bool bold = Convert.ToBoolean(dstvcOld.VarValueAsObject);
                            bool bnew = Convert.ToBoolean(dstvNew.VarValueAsObject);
                            rez = bold == bnew ? true : false;
                            break;
                        case "System.Single":
                            Single sold = Convert.ToSingle(dstvcOld.VarValueAsObject);
                            Single snew = Convert.ToSingle(dstvNew.VarValueAsObject);
                            rez = sold == snew ? true : false;
                            break;
                        case "System.Double":
                            Double Doubleold = Convert.ToDouble(dstvcOld.VarValueAsObject);
                            Double Doublenew = Convert.ToDouble(dstvNew.VarValueAsObject);
                            rez = Doubleold == Doublenew ? true : false;
                            break;
                        case "System.Int16":
                            Int16 i16old = Convert.ToInt16(dstvcOld.VarValueAsObject);
                            Int16 i16new = Convert.ToInt16(dstvNew.VarValueAsObject);
                            rez = i16old == i16new ? true : false;
                            break;
                        case "System.UInt16":
                            UInt16 ui16old = Convert.ToUInt16(dstvcOld.VarValueAsObject);
                            UInt16 ui16new = Convert.ToUInt16(dstvNew.VarValueAsObject);
                            rez = ui16old == ui16new ? true : false;
                            break;
                        case "System.Int32":
                            Int32 i32old = Convert.ToInt32(dstvcOld.VarValueAsObject);
                            Int32 i32new = Convert.ToInt32(dstvNew.VarValueAsObject);
                            rez = i32old == i32new ? true : false;
                            break;
                        case "System.UInt32":
                            UInt32 UInt32old = Convert.ToUInt32(dstvcOld.VarValueAsObject);
                            UInt32 UInt32new = Convert.ToUInt32(dstvNew.VarValueAsObject);
                            rez = UInt32old == UInt32new ? true : false;
                            break;

                         case "System.String":
                            string stold = Convert.ToString(dstvcOld.VarValueAsObject);
                            string stnew = Convert.ToString(dstvNew.VarValueAsObject);
                            rez = stold == stnew ? true : false;
                            break;
                        case "System.DateTime":
                            DateTime dold = Convert.ToDateTime(dstvcOld.VarValueAsObject);
                            DateTime dnew = Convert.ToDateTime(dstvNew.VarValueAsObject);
                            rez = dold == dnew ? true : false;
                            break;
                        case "System.Byte[]":
                            rez = CompareMemXs(dstvcOld.VarValueAsObject as byte[],dstvNew.VarValueAsObject as byte[]) ? false : true; // if равен то true
                            break;
                        default:
                            throw new Exception(string.Format(@"(249) ...\00_DataServer\CommonClasses\CommonUtils.cs: CompareTagAsObjects() : Тип (type4OldValue = {0}) не поддерживается.", type4OldValue));
                    }
                }

                // посмотрим еще и качество
                rezq = dstvcOld.VarQuality == dstvNew.VarQuality ? true : false;
                if (!rezq)
                    rez = rezq; // результат - считаем - теги не равны
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return rez;
        }
        /// <summary>
        /// сравнить два байтовых массива
        /// </summary>
        public static bool CompareMemXs(byte[] tmp, byte[] p)
        {
            bool rez = false;
            try
            {
                if (tmp.Length != p.Length)
                {
                }
                else
                {
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        if (tmp[i] != p[i])
                        {
                            rez = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }

        #endregion

        #region функции для работы с формулами
        /// <summary>
        /// выделить из формулы подстроки вида ds.dev.tagguid
        /// для выполнение подписки на обновление тегов
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="numrtu">устройство для подстановки вместо dev, если в запросе dev = 0 , т.е. локальное</param>
        public static List<string> ParseFormula4ExtractTagDescribe(string formula, UInt32 UniDS_GUID, UInt32 numrtu)//, ChangeCalcTag cct
        {
            List<string> listFrmlTags = new List<string>();

            /*
             * используем регулярное выражение для выделения 
             * подстрок вида цифры.цифры.цифры
             */
            try
            {
                Regex re = new Regex(@"[\d]+\.[\d]+\.[\d]+");
                MatchCollection mc = re.Matches(formula);
                int iCountMatchs = mc.Count;

                // заменить ds->UniDS_GUID и dev->numrtu
                ArrayList ar = new ArrayList();
                StringBuilder sb = new StringBuilder();

                foreach (Match m in mc)
                {
                    /*
                     * анализируем на локальность
                     */
                    string[] strloc = m.Value.Split(new char[] { '.' });

                    sb.Clear();

                    if (strloc[1] == "0")
                        sb.Append(Regex.Replace(m.Value, @"\.[\d]+\.", string.Format(".{0}.", numrtu.ToString())));
                    else
                        sb.Append(m.Value);

                    ar.Add(Regex.Replace(sb.ToString(), @"^[\d]+\.", string.Format("{0}.", UniDS_GUID)));
                }

                //добавляем
                foreach (string st in ar)
                    if (!listFrmlTags.Contains(st))
                        listFrmlTags.Add(st);
                    else
                        continue;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return listFrmlTags;
        }
        
        #endregion
    }
}
