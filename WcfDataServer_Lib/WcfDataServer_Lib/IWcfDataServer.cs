using InterfaceLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace WcfDataServer_Lib
{
    /* 
     * Класс GeneratedCodeAttribute может использоваться средствами анализа кода для идентификации кода, 
     * сгенерированного компьютером, и предоставления анализа на основе средства, сгенерировавшего код,
     * и версии этого средства.
     */
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    /*
     * ServiceContract - определяет интерфейс как контракт службы
     *  CallbackContract - Получает или задает тип обратного вызова, когда соглашение является дуплексной моделью
     *  ConfigurationName - Получает или задает имя, используемое для поиска службы в файле конфигурации приложения
     *  SessionMode - Получает или задает, разрешены ли сессии, не разрешены, или требуется
     */
    [System.ServiceModel.ServiceContract(ConfigurationName = "IWcfDataServer", SessionMode = System.ServiceModel.SessionMode.Required, CallbackContract = typeof(IWcfDataServerCallback))]    
    public interface IWcfDataServer
    {
        /// <summary>
        /// Выполнить команду
        /// </summary>
        /// <param name="numksdu">ds</param>
        /// <param name="numvtu">rtuguid</param>
        /// <param name="tagguid">tagguid команды</param>
        /// <param name="arr">опция - массив параметров</param>
        /// <returns>успешность запуска команды на выполнение</returns>
        [OperationContract]
        void RunCMD(UInt16 numksdu, uint numvtu, uint tagguid, byte[] arr, string idDSRouterSession);

        #region информация об DataServer’ах
        /// <summary>
        /// уникальный идентификатор DataServer'a DSGuid
        /// </summary>
        /// <returns></returns>
 [OperationContract]
        string GetDSGUID();
        /// <summary>
        /// информация об имени DataServer конкретного DataServer: 
        /// возвращает имя проекта, которое извлекается из файла
        /// …\Project\Project.cfg xml-секция Project|NamePTK
        /// </summary>
        /// <returns></returns>
 [OperationContract]
        string GetDSINFO();
        /// <summary>
        /// информация о конфигурации DataServer
        /// в виде файла
        /// </summary>       
 [OperationContract]
        Stream GetDSConfigFile();
        #endregion

        #region информация об источниках конкретного DataServer
        /// <summary>
        /// список идентификаторов источников 
        /// указанного DataServer
        /// в формате SrcGuid; SrcGuid; SrcGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetSourceGUIDs();
        /// <summary>
        /// возвращает имя источника SrcGuid
        /// DataServer DSGuid
        /// </summary>
        /// <param name="SrcGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetSourceName(UInt16 SrcGuid);
	    #endregion

        #region информация о контроллерах конкретного источника конкретного DataServer
        /// <summary>
        /// список идентификаторов контроллеров источника SrcGuid 
        /// DataServer DSGuid в формате EcuGuid; EcuGuid; EcuGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetECUGUIDs(UInt16 SrcGuid);
        /// <summary>
        /// возвращает имя источника SrcGuid
        /// для  DataServer DSGuid.
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <returns></returns> 
 [OperationContract]
        string GetECUName(UInt16 SrcGuid, UInt16 EcuGuid);
        #endregion

        #region информация об устройствах контроллера ECU источника SrcGuid DataServer DSGuid		
        /// <summary>
        /// список идентификаторов устройств 
        /// контроллера EcuGuid источника SrcGuid 
        /// DataServer DSGuid 
        /// в формате RtuGuid; RtuGuid; RtuGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetSrcEcuRTUGUIDs(UInt16 SrcGuid, UInt16 EcuGuid);
        /// <summary>
        /// список идентификаторов устройств 
        /// данного DataServer 
        /// в формате RtuGuid; RtuGuid; RtuGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetRTUGUIDs();
        /// <summary>
        /// имя типа устройства
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetRTUTypeName(UInt32 RtuGuid);
        /// <summary>
        /// строка описания устройства
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetRTUDescription(UInt32 RtuGuid);
        /// <summary>
        /// признак доступности устройства для обработки
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
 [OperationContract]
        bool IsRTUEnable(UInt32 RtuGuid); 
	    #endregion

        #region информация о группах устройства RtuGuid контроллера ECU источника SrcGuid DataServer DSGuid		        
        /// <summary>
        /// список групп первого уровня в формате
        /// GroupGuid; GroupGuid; GroupGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetGroupGUIDs(UInt32 RtuGuid);
        /// <summary>
        /// список подгрупп группы(подгруппы) (Sub)GroupGuid
        /// в формате SubGroupGuid; SubGroupGuid; SubGroupGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="GroupGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetSubGroupGUIDsInGroup(UInt32 RtuGuid, UInt32 GroupGuid);
        /// <summary>
        /// имя группы GroupGuid устройства RtuGuid
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="GroupGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetRTUGroupName(UInt32 RtuGuid, UInt32 GroupGuid);
        /// <summary>
        /// признак доступности группы для обработки
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="GroupGuid"></param>
        /// <returns></returns>
 [OperationContract]
        bool IsGroupEnable(UInt32 RtuGuid, UInt32 GroupGuid); 
    	#endregion

        #region информация о тегах устройства RtuGuid контроллера ECU источника SrcGuid DataServer DSGuid		
        /// <summary>
        /// список всех тегов устройства в формате TagGuid; TagGuid; TagGuid… 
        /// Список может быть очень большим, поэтому доступ к тегам можно получить
        /// через иерархию групп с помощью следующей функции:
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetRtuTagGUIDs(UInt32 RtuGuid);
        /// <summary>
        /// список тегов группы GroupGuid устройства
        /// RtuGuid в формате TagGuid; TagGuid; TagGuid… 
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="GroupGuid"></param>
        /// <returns></returns>
 [OperationContract]
        string GetRtuGroupTagGUIDs(UInt32 RtuGuid, UInt32 GroupGuid);
        /// <summary>
        /// информация о имени и типе тега в формате
        /// имя_тега;тип_тега
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="GroupGuid"></param>
        /// <param name="TagGUID"></param>
        /// <returns></returns>
 [OperationContract]
        string GetRTUTagName(UInt32 RtuGuid, UInt32 TagGUID);
        /// <summary>
        /// запрос для получения значения тега(ов)однократно.
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
 [OperationContract]
        byte[] GetRTUTagsValue(UInt32 RtuGuid, byte[] request);
        ///// <summary>
        ///// список тегов группы GroupGuid устройства
        ///// RtuGuid в формате TagGuid; TagGuid; TagGuid… 
        ///// </summary>
        ///// <param name="DSGuid"></param>
        ///// <param name="SrcGuid"></param>
        ///// <param name="EcuGuid"></param>
        ///// <param name="RtuGuid"></param>
        ///// <param name="GroupGuid"></param>
        ///// <param name="TagGuid"></param>
        ///// <returns></returns>
        //[OperationContract (IsOneWay = true)]
        //bool IsTagEnable(string DSGuid, string SrcGuid, string EcuGuid, string RtuGuid, string GroupGuid, string TagGuid); 
        /// <summary>
        /// запрос для получения тегов однократно
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, DSTagValue> GetTagsValue(List<string> request);
        /// <summary>
        /// подписаться на теги
        /// </summary>
        /// <param name="request"></param>
        [OperationContract]
        void SubscribeRTUTags(List<string> request);
        /// <summary>
        /// отписаться от обновления тегов
        /// </summary>
        /// <param name="request"></param>
        [OperationContract(IsOneWay = true)]
        void UnscribeRTUTags(List<string> request);
	    #endregion

        #region поддержка функционирования старого арма - обмен пакетами
        /// <summary>
        /// функция для принятия больших запросов
        /// с возможностью их соединения после разбиения на 
        /// стороне клиента
        /// </summary>
        /// <param name="arr">запрос</param>
        /// <param name="currentNumberPacket">номер текущего сегмента общего пакета</param>
        /// <param name="packetCount">число сегментов большого пакета</param>
        /// <returns></returns>
        [OperationContract]
        MemoryStream GetDSValueAsPartialByteBuffer(byte[] arr, int currentNumberPacket, int packetCount);
        [OperationContract]
        MemoryStream GetDSValueAsByteBuffer(byte[] arr); 
        /// <summary>
        /// запрос осциллограммы - содедржимое
        /// запроса в структуре, скрытой в массиве байт
        /// </summary>
        /// <param name="pq"></param>
        /// <returns></returns>
        [OperationContract]
        MemoryStream GetDSOscByIdInBD(byte[] pq);
        /// <summary>
        /// запрос на отображение архивной информации -
        /// запрос архивных данных (по номерам записей в БД) -
        /// уставки, аварии
        /// </summary>
        /// <param name="pq"></param>
        /// <returns></returns>
        [OperationContract]
        void SetReq2ArhivInfo(byte[] pq);
        /// <summary>
        /// выполнить команду
        /// </summary>
        /// <param name="pq"></param>
        /// <returns></returns>
        [OperationContract]
        MemoryStream RunCMDMOA(byte[] pq);
        #endregion

        #region функции общего назначения
        /// <summary>
        /// получить коды последних ошибок
        /// (стек ошибок) при обмене с DS
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        LstError GetDSLastErrorsGUID();
         /// <summary>
        /// получить код последней ошибки при обмене с DS
        /// в формате code@timestamp
        /// </summary>
        /// <returns></returns>
         [OperationContract]
        string GetDSLastErrorGUID();
        /// <summary>
        /// получить текст ошибки 
        /// при обмене с DS по ее коду
        /// </summary>
        /// <param name="lastErrorGUID"></param>
        /// <returns></returns>
        [OperationContract]
        string GetDSErrorTextByErrorGUID(string errorGUID);
        /// <summary>
        /// квитировать (очистить)
        /// стек ошибок
        /// </summary>
        [OperationContract (IsOneWay = true)]
        void AcknowledgementOfErrors();

        /// <summary>
        /// регистрация клиента для механизма
        /// callback оповещения о новой ошибке
        /// </summary>
        /// <param name="keyticker"></param>
        [OperationContract (IsOneWay = true)]
        void RegisterForErrorEvent(string keyticker);
        /// <summary>
        /// ping
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Ping();
        #endregion

        #region ручной ввод данных
        /// <summary>
        /// Установить значение тега 
        /// с уровня HMI через тип object
        /// (качество тега vqHandled)
        /// </summary>
        /// <param name="arrTagValue"></param>
        [OperationContract(IsOneWay = true)]
        void SetTagValueFromHMI(string idTag, object valinobject);
        /// <summary>
        /// восстановить процесс естесвенного обновления тега
        /// (качество тега vqGood или по факту)
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ReSetTagValueFromHMI(string idTag); 
	    #endregion

        #region Ручной ввод преобразовывающих коэффициентов
        /// <summary>
        /// Получить коэффициент преобразования для тега
        /// </summary>
        [OperationContract]
        Object GetTagAnalogTransformationRatio(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid);

        /// <summary>
        /// Установить коэффициент преобразования
        /// </summary>
        [OperationContract]
        void SetTagAnalogTransformationRatio(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid, Object transformationRatio);

        /// <summary>
        /// Сбросить коэффициент преобразования
        /// </summary>
        [OperationContract]
        void ReSetTagAnalogTransformationRatio(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid);

        /// <summary>
        /// Возвращает true, если значение дискретного тега инверсируется
        /// </summary>
        [OperationContract]
        bool IsInverseDiscretTag(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid);

        /// <summary>
        /// Инверсирует значение дискретного тега
        /// </summary>
        [OperationContract]
        void InverseDiscretTag(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid, bool newInverseProperty);
        #endregion

        #region Новые функции

        #region Работа с пользователем

        /// <summary>
        /// Метод авторизации пользователя
        /// </summary>
        [OperationContract]
        DSAuthResult Authorization(string userName, string userPassword, bool isFirstAuthorization, DSUserSessionInfo userSessionInfo);

        /// <summary>
        /// Получить список пользователей
        /// </summary>
        [OperationContract]
        List<DSUser> GetUsersList();

        /// <summary>
        /// Получить список групп пользователей
        /// </summary>
        [OperationContract]
        List<DSUserGroup> GetUserGroupsList();

        /// <summary>
        /// Создание группы пользователей
        /// </summary>
        [OperationContract]
        Boolean CreateUserGroup(string groupName, string groupComment, string groupRight, DSUserSessionInfo userSessionInfo);

        /// <summary>
        /// Создание пользователя
        /// </summary>
        [OperationContract]
        Boolean CreateUser(string userName, string userPassword, string userComment, Int32 userGroupID, DSUserSessionInfo userSessionInfo);

        #endregion

        #region Работа с событиями

        #region Запрос событий

        /// <summary>
        /// Получение событий
        /// </summary>
        [OperationContract]
        List<DSEventValue> GetEvents(DateTime dateTimeFrom, DateTime dateTimeTo, bool needSystemEvents, bool needUserEvents, bool needTerminalEvents, List<UInt32> devicesList);

        #endregion

        #region Работа с данными

        /// <summary>
        /// Получить содержимое осциллограммы по её номеру
        /// </summary>
        [OperationContract]
        DSOscillogram GetOscillogramByID(Int32 eventDataID);

        /// <summary>
        /// Получить архивную информацию (аварии, уставки и т.д.) как словарь значений
        /// </summary>
        [OperationContract]
        Dictionary<string, DSTagValue> GetHistoricalDataByID(Int32 dataID);

        #endregion

        #region Работа с квитированием

        /// <summary>
        /// Проверяет есть ли не кватированные аварийтные сообщения
        /// </summary>
        [OperationContract]
        Boolean IsNotReceiptedEventsExist();

        /// <summary>
        /// Получить не квитированные аварийные сообщения
        /// </summary>
        [OperationContract]
        List<DSEventValue> GetNotReceiptedEvents();

        /// <summary>
        /// Квитировать сообщения
        /// </summary>
        [OperationContract]
        void ReceiptEvents(List<Int32> eventValuesId, Int32 userID, string receiptComment, DSUserSessionInfo userSessionInfo);

        /// <summary>
        /// Квитировать все сообщения
        /// </summary>
        [OperationContract]
        void ReceiptAllEvents(Int32 userID, string receiptComment, DSUserSessionInfo userSessionInfo);

        #endregion

        #endregion

        #region Уставки

        /// <summary>
        /// Получение списка архивных записей уставок для устройства
        /// </summary>
        [OperationContract]
        List<DSSettingsSet> GetSettingsSetsList(UInt32 devGuid);

        /// <summary>
        /// Получение значений для указанных тегов из конкретного архивного набора уставок
        /// </summary>
        [OperationContract]
        Dictionary<String, DSTagValue> GetValuesFromSettingsSet(Int32 settingsSetID);

        /// <summary>
        /// Запись набора уставкок в устройство
        /// </summary>
        [OperationContract]
        void SaveSettingsToDevice(UInt32 devGuid, Dictionary<string, DSTagValue> tagsValues);

        #endregion

        #region Комманды
        /// <summary>
        /// <summary>
        /// Запрос на запуск команды на устройстве (SAF IEC)
        /// </summary>
        /// <param name="ACommandID">ds.dev.cmdid</param>
        /// <param name="AParameters">массив параметров</param>
        /// <param name="idDSRouterSession">идентификатор сессии на DSRouter кот выдал команду</param>
        /// <returns></returns>
        [OperationContract]
        void CommandRun(string ACommandID, object[] AParameters, string idDSRouterSession);
        /// <summary>
        /// проверка статуса выполнения команды
        /// для конкретного клиента
        /// </summary>
        /// <param name="ACommandID"></param>
        /// <returns></returns>
        [OperationContract]
        EnumerationCommandStates CommandStateCheck(string idDSRouterSession);
        #endregion

        #region Документы

        #region Load

        /// <summary>
        /// Получить список документов терминала
        /// </summary>
        [OperationContract]
        List<DSDocumentDataValue> GetDocumentsList(Int32 devGuid);

        /// <summary>
        /// Получить ссылку на документ
        /// </summary>
        [OperationContract]
        DSDataFile GetDocumentByID(Int32 documentId);

        #endregion

        #region Upload

        /// <summary>
        /// Загрузить кусочек файла
        /// </summary>
        [OperationContract]
        bool UploadFileChunk(byte[] fileChunk);

        /// <summary>
        /// Сохранить файл
        /// </summary>
        [OperationContract]
        bool SaveUploadedFile(Int32 devGuid, Int32 userId, string fileName, string fileComment);

        /// <summary>
        /// Отменяет загрузку файлов
        /// </summary>
        [OperationContract]
        void TerminateUploadFileSession();

        #endregion

        #endregion

        #region Работа с тревогами
        /// <summary>
        /// Получить список неквитированных 
        /// событий (заданное количество)
        /// </summary>
        [OperationContract]
        List<DSAlarmsInfo> GetLastNonConfirmMAlarms(Int32 count);
        /// <summary>

        /// квиитрование
        /// </summary>
        /// <param name="EventGuid"></param>
        /// <param name="ECCComment"></param>
        [OperationContract]
        void ConfirmAlarm(string EventGuid, string ECCComment);
        /// <summary>
        /// запрос списка тревог из диапазона дат
        /// </summary>
        /// <param name="DTStart"></param>
        /// <param name="DTEnd"></param>
        /// <returns></returns>
        [OperationContract]
        List<DSAlarmsInfo> GetAlarmsInDatesRange(DateTime DTStart, DateTime DTEnd);
        #endregion

        #region Тренды
        /// <summary>
        /// Получить список тегов, у которых включена запись значений
        /// </summary>
        [OperationContract]
        List<string> GetTagsListWithEnabledTrendSave();

        /// <summary>
        /// Получить диапазоны времени, в которых доступны данные
        /// </summary>
        [OperationContract]
        List<Tuple<DateTime, DateTime>> GetTagTrendDateTimeRanges(uint devGuid, uint tagGuid);

        /// <summary>
        /// Получить тренд единым списком
        /// </summary>
        [OperationContract]
        List<Tuple<DateTime, object>> GetTagTrend(uint devGuid, uint tagGuid, DateTime startDateTime, DateTime endDateTime);

        /// <summary>
        /// Получить список обособленных трендов
        /// </summary>
        [OperationContract]
        List<List<Tuple<DateTime, object>>> GetTagTrendsList(uint devGuid, uint tagGuid, DateTime startDateTime, DateTime endDateTime);

        /// <summary>
        /// Получить настройки режима работы записи тренда
        /// </summary>
        [OperationContract]
        DSTrendSettings GetTrendSettings(uint devGuid, uint tagGuid);

        /// <summary>
        /// Установить настройки режима работы записи тренда
        /// </summary>
        [OperationContract]
        void SetTrendSettings(uint devGuid, uint tagGuid, DSTrendSettings trendSettings);

        #endregion

        #endregion
    }

    #region DataContract's

    #endregion
    #region Тревоги
    [DataContract]
    public class DSAlarmsInfo
    {
        /// <summary>
        /// уник идент тревоги
        /// </summary>
        [DataMember]
        public string GuidAlarmRecord { get; set; }
        /// <summary>
        /// признак квитирования
        /// </summary>
        [DataMember]
        public bool CONFIRM { get; set; }
        /// <summary>
        /// строка идентификации тега
        /// </summary>
        [DataMember]
        public string StrTagId { get; set; }
        /// <summary>
        /// идентификатор устр
        /// </summary>
        [DataMember]
        public string devguid { get; set; }
        /// <summary>
        /// идентификатор тега
        /// </summary>
        [DataMember]
        public string tagguid { get; set; }
        /// <summary>
        /// уровень тревоги
        /// </summary>
        [DataMember]
        public uint ALARMLEVEL { get; set; }
        /// <summary>
        /// текст сообщения тревоги
        /// </summary>
        [DataMember]
        public string ALARMTEXTMESSAGE { get; set; }
        /// <summary>
        /// время тревоги
        /// </summary>
        [DataMember]
        public DateTime ALARMTIMESTAMP { get; set; }
        /// <summary>
        /// комментарий тревоги
        /// </summary>
        [DataMember]
        public string COMMENT { get; set; }
    }    
    #endregion

    #region Callback

    [ServiceContract]
    public interface IWcfDataServerCallback
    {
        /// <summary>
        /// оповещение клиента о возникновении ошибки
        /// </summary>
        /// <param name="codeDataTimeEvent"></param>
        [OperationContract(IsOneWay = true)]
        void NewErrorEvent(string codeDataTimeEvent);

        [OperationContract(IsOneWay = true)]
        void Pong();

        /// <summary>
        /// оповещение клиента об изменении тегов 
        /// (по подписке)
        /// </summary>
        /// <param name="codeDataTimeEvent"></param>
        [OperationContract(IsOneWay = true)]
        void NotifyChangedTags(Dictionary<string, DSTagValue> lstChangedTags);

        /// <summary>
        /// извешение о выполнении команды
        /// </summary>
        /// <param name="cmdarray"></param>
        [OperationContract(IsOneWay = true)]
        void NotifyCMDExecuted(byte[] cmdarray);
    }

    #endregion
}
