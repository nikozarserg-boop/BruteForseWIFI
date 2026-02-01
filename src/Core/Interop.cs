#pragma warning disable 1573
#pragma warning disable 1574
using System;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace NativeWifi
{
	public static class Wlan
	{
		// Нативное API Windows
		public enum WlanIntfOpcode
		{
			AutoconfEnabled = 1,
			BackgroundScanEnabled,
			MediaStreamingMode,
			RadioState,
			BssType,
			InterfaceState,
			CurrentConnection,
			ChannelNumber,
			SupportedInfrastructureAuthCipherPairs,
			SupportedAdhocAuthCipherPairs,
			SupportedCountryOrRegionStringList,
			CurrentOperationMode,
			Statistics = 0x10000101,
			RSSI,
			SecurityStart = 0x20010000,
			SecurityEnd = 0x2fffffff,
			IhvStart = 0x30000000,
			IhvEnd = 0x3fffffff
		}

		public enum WlanOpcodeValueType
		{
			QueryOnly = 0,
			SetByGroupPolicy = 1,
			SetByUser = 2,
			Invalid = 3
		}

		public const uint WLAN_CLIENT_VERSION_XP_SP2 = 1;
		public const uint WLAN_CLIENT_VERSION_LONGHORN = 2;

		[DllImport("wlanapi.dll")]
		public static extern int WlanOpenHandle(
			[In] UInt32 clientVersion,
			[In, Out] IntPtr pReserved,
			[Out] out UInt32 negotiatedVersion,
			[Out] out IntPtr clientHandle);

		[DllImport("wlanapi.dll")]
		public static extern int WlanCloseHandle(
			[In] IntPtr clientHandle,
			[In, Out] IntPtr pReserved);

		[DllImport("wlanapi.dll")]
		public static extern int WlanEnumInterfaces(
			[In] IntPtr clientHandle,
			[In, Out] IntPtr pReserved,
			[Out] out IntPtr ppInterfaceList);

		[DllImport("wlanapi.dll")]
		public static extern int WlanQueryInterface(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanIntfOpcode opCode,
			[In, Out] IntPtr pReserved,
			[Out] out int dataSize,
			[Out] out IntPtr ppData,
			[Out] out WlanOpcodeValueType wlanOpcodeValueType);

		[DllImport("wlanapi.dll")]
		public static extern int WlanSetInterface(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanIntfOpcode opCode,
			[In] uint dataSize,
			[In] IntPtr pData,
			[In, Out] IntPtr pReserved);

		[DllImport("wlanapi.dll")]
		public static extern int WlanScan(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] IntPtr pDot11Ssid,
			[In] IntPtr pIeData,
			[In, Out] IntPtr pReserved);

		[Flags]
		public enum WlanGetAvailableNetworkFlags
		{
			IncludeAllAdhocProfiles = 0x00000001,
			IncludeAllManualHiddenProfiles = 0x00000002
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct WlanAvailableNetworkListHeader
		{
			public uint numberOfItems;
			public uint index;
		}

		[Flags]
		public enum WlanAvailableNetworkFlags
		{
			Connected = 0x00000001,
			HasProfile = 0x00000002
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public struct WlanAvailableNetwork
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string profileName;
			public Dot11Ssid dot11Ssid;
			public Dot11BssType dot11BssType;
			public uint numberOfBssids;
			public bool networkConnectable;
			public WlanReasonCode wlanNotConnectableReason;
			private uint numberOfPhyTypes;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			private Dot11PhyType[] dot11PhyTypes;
			public Dot11PhyType[] Dot11PhyTypes
			{
				get
				{
					Dot11PhyType[] ret = new Dot11PhyType[numberOfPhyTypes];
					Array.Copy(dot11PhyTypes, ret, numberOfPhyTypes);
					return ret;
				}
			}
			public bool morePhyTypes;
			public uint wlanSignalQuality;
			public bool securityEnabled;
			public Dot11AuthAlgorithm dot11DefaultAuthAlgorithm;
			public Dot11CipherAlgorithm dot11DefaultCipherAlgorithm;
			public WlanAvailableNetworkFlags flags;
			uint reserved;
		}

		[DllImport("wlanapi.dll")]
		public static extern int WlanGetAvailableNetworkList(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanGetAvailableNetworkFlags flags,
			[In, Out] IntPtr reservedPtr,
			[Out] out IntPtr availableNetworkListPtr);

		[Flags]
		public enum WlanProfileFlags
		{
			AllUser = 0,
			GroupPolicy = 1,
			User = 2
		}

		[DllImport("wlanapi.dll")]
		public static extern int WlanSetProfile(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] WlanProfileFlags flags,
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileXml,
			[In, Optional, MarshalAs(UnmanagedType.LPWStr)] string allUserProfileSecurity,
			[In] bool overwrite,
			[In] IntPtr pReserved,
			[Out] out WlanReasonCode reasonCode);

		[Flags]
		public enum WlanAccess
		{
			ReadAccess = 0x00020000 | 0x0001,
			ExecuteAccess = ReadAccess | 0x0020,
			WriteAccess = ReadAccess | ExecuteAccess | 0x0002 | 0x00010000 | 0x00040000
		}

		[DllImport("wlanapi.dll")]
		public static extern int WlanGetProfile(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileName,
			[In] IntPtr pReserved,
			[Out] out IntPtr profileXml,
			[Out, Optional] out WlanProfileFlags flags,
			[Out, Optional] out WlanAccess grantedAccess);

		[DllImport("wlanapi.dll")]
		public static extern int WlanGetProfileList(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] IntPtr pReserved,
			[Out] out IntPtr profileList
		);

		[DllImport("wlanapi.dll")]
		public static extern void WlanFreeMemory(IntPtr pMemory);

		[DllImport("wlanapi.dll")]
		public static extern int WlanReasonCodeToString(
			[In] WlanReasonCode reasonCode,
			[In] int bufferSize,
			[In, Out] StringBuilder stringBuffer,
			IntPtr pReserved
		);

		[Flags]
		public enum WlanNotificationSource
		{
			None = 0,
			All = 0X0000FFFF,
			ACM = 0X00000008,
			MSM = 0X00000010,
			Security = 0X00000020,
			IHV = 0X00000040
		}

		public enum WlanNotificationCodeAcm
		{
			AutoconfEnabled = 1,
			AutoconfDisabled,
			BackgroundScanEnabled,
			BackgroundScanDisabled,
			BssTypeChange,
			PowerSettingChange,
			ScanComplete,
			ScanFail,
			ConnectionStart,
			ConnectionComplete,
			ConnectionAttemptFail,
			FilterListChange,
			InterfaceArrival,
			InterfaceRemoval,
			ProfileChange,
			ProfileNameChange,
			ProfilesExhausted,
			NetworkNotAvailable,
			NetworkAvailable,
			Disconnecting,
			Disconnected,
			AdhocNetworkStateChange
		}

		public enum WlanNotificationCodeMsm
		{
			Associating = 1,
			Associated,
			Authenticating,
			Connected,
			RoamingStart,
			RoamingEnd,
			RadioStateChange,
			SignalQualityChange,
			Disassociating,
			Disconnected,
			PeerJoin,
			PeerLeave,
			AdapterRemoval,
			AdapterOperationModeChange
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WlanNotificationData
		{
			public WlanNotificationSource notificationSource;
			public int notificationCode;
			public Guid interfaceGuid;
			public int dataSize;
			public IntPtr dataPtr;

			public object NotificationCode
			{
				get
				{
					switch (notificationSource)
					{
						case WlanNotificationSource.MSM:
							return (WlanNotificationCodeMsm)notificationCode;
						case WlanNotificationSource.ACM:
							return (WlanNotificationCodeAcm)notificationCode;
						default:
							return notificationCode;
					}
				}
			}
		}

		public delegate void WlanNotificationCallbackDelegate(ref WlanNotificationData notificationData, IntPtr context);

		[DllImport("wlanapi.dll")]
		public static extern int WlanRegisterNotification(
			[In] IntPtr clientHandle,
			[In] WlanNotificationSource notifSource,
			[In] bool ignoreDuplicate,
			[In] WlanNotificationCallbackDelegate funcCallback,
			[In] IntPtr callbackContext,
			[In] IntPtr reserved,
			[Out] out WlanNotificationSource prevNotifSource);

		[Flags]
		public enum WlanConnectionFlags
		{
			HiddenNetwork = 0x00000001,
			AdhocJoinOnly = 0x00000002,
			IgnorePrivacyBit = 0x00000004,
			EapolPassthrough = 0x00000008
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WlanConnectionParameters
		{
			public WlanConnectionMode wlanConnectionMode;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string profile;
			public IntPtr dot11SsidPtr;
			public IntPtr desiredBssidListPtr;
			public Dot11BssType dot11BssType;
			public WlanConnectionFlags flags;
		}

		public enum WlanAdhocNetworkState
		{
			Formed = 0,
			Connected = 1
		}

		[DllImport("wlanapi.dll")]
		public static extern int WlanConnect(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] ref WlanConnectionParameters connectionParameters,
			IntPtr pReserved);

		[DllImport("wlanapi.dll")]
		public static extern int WlanDeleteProfile(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In, MarshalAs(UnmanagedType.LPWStr)] string profileName,
			IntPtr reservedPtr
		);

		[DllImport("wlanapi.dll")]
		public static extern int WlanGetNetworkBssList(
			[In] IntPtr clientHandle,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
			[In] IntPtr dot11SsidInt,
			[In] Dot11BssType dot11BssType,
			[In] bool securityEnabled,
			IntPtr reservedPtr,
			[Out] out IntPtr wlanBssList
		);

		[StructLayout(LayoutKind.Sequential)]
		internal struct WlanBssListHeader
		{
			internal uint totalSize;
			internal uint numberOfItems;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WlanBssEntry
		{
			public Dot11Ssid dot11Ssid;
			public uint phyId;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] dot11Bssid;
			public Dot11BssType dot11BssType;
			public Dot11PhyType dot11BssPhyType;
			public int rssi;
			public uint linkQuality;
			public bool inRegDomain;
			public ushort beaconPeriod;
			public ulong timestamp;
			public ulong hostTimestamp;
			public ushort capabilityInformation;
			public uint chCenterFrequency;
			public WlanRateSet wlanRateSet;
			public uint ieOffset;
			public uint ieSize;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WlanRateSet
		{
			private uint rateSetLength;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 126)]
			private ushort[] rateSet;

			public ushort[] Rates
			{
				get
				{
					ushort[] rates = new ushort[rateSetLength / sizeof(ushort)];
					Array.Copy(rateSet, rates, rates.Length);
					return rates;
				}
			}

			public double GetRateInMbps(int rateIndex)
			{
				if ((rateIndex < 0) || (rateIndex > rateSet.Length))
					throw new ArgumentOutOfRangeException("rateIndex");

				return (rateSet[rateIndex] & 0x7FFF) * 0.5;
			}
		} 

		public class WlanException : Exception
		{
			private readonly WlanReasonCode reasonCode;

			public WlanException(WlanReasonCode reasonCode)
			{
				this.reasonCode = reasonCode;
			}

			public WlanReasonCode ReasonCode
			{
				get { return reasonCode; }
			}

			public override string Message
			{
				get
				{
					StringBuilder sb = new StringBuilder(1024);
					return
						WlanReasonCodeToString(reasonCode, sb.Capacity, sb, IntPtr.Zero) == 0 ?
							sb.ToString() :
							string.Empty;
				}
			}
		}

		// TODO: Адаптировать enum WlanReasonCode под .NET (соглашение + docs).

		public enum WlanReasonCode
		{
			Success = 0,
			// Основные коды
			UNKNOWN = 0x10000 + 1,

			RANGE_SIZE = 0x10000,
			BASE = 0x10000 + RANGE_SIZE,

			// Диапазон автоконфигурации
			//
			AC_BASE = 0x10000 + RANGE_SIZE,
			AC_CONNECT_BASE = (AC_BASE + RANGE_SIZE / 2),
			AC_END = (AC_BASE + RANGE_SIZE - 1),

			// Диапазон менеджера профилей
			// содержит коды ошибок добавления профилей, может не иметь 
			// кодов ошибок подключения
			//
			PROFILE_BASE = 0x10000 + (7 * RANGE_SIZE),
			PROFILE_CONNECT_BASE = (PROFILE_BASE + RANGE_SIZE / 2),
			PROFILE_END = (PROFILE_BASE + RANGE_SIZE - 1),

			// Диапазон MSM
			//
			MSM_BASE = 0x10000 + (2 * RANGE_SIZE),
			MSM_CONNECT_BASE = (MSM_BASE + RANGE_SIZE / 2),
			MSM_END = (MSM_BASE + RANGE_SIZE - 1),

			// Диапазон MSMSEC
			//
			MSMSEC_BASE = 0x10000 + (3 * RANGE_SIZE),
			MSMSEC_CONNECT_BASE = (MSMSEC_BASE + RANGE_SIZE / 2),
			MSMSEC_END = (MSMSEC_BASE + RANGE_SIZE - 1),

			// Коды несовместимости сети AC
			//
			NETWORK_NOT_COMPATIBLE = (AC_BASE + 1),
			PROFILE_NOT_COMPATIBLE = (AC_BASE + 2),

			// Коды подключения AC
			//
			NO_AUTO_CONNECTION = (AC_CONNECT_BASE + 1),
			NOT_VISIBLE = (AC_CONNECT_BASE + 2),
			GP_DENIED = (AC_CONNECT_BASE + 3),
			USER_DENIED = (AC_CONNECT_BASE + 4),
			BSS_TYPE_NOT_ALLOWED = (AC_CONNECT_BASE + 5),
			IN_FAILED_LIST = (AC_CONNECT_BASE + 6),
			IN_BLOCKED_LIST = (AC_CONNECT_BASE + 7),
			SSID_LIST_TOO_LONG = (AC_CONNECT_BASE + 8),
			CONNECT_CALL_FAIL = (AC_CONNECT_BASE + 9),
			SCAN_CALL_FAIL = (AC_CONNECT_BASE + 10),
			NETWORK_NOT_AVAILABLE = (AC_CONNECT_BASE + 11),
			PROFILE_CHANGED_OR_DELETED = (AC_CONNECT_BASE + 12),
			KEY_MISMATCH = (AC_CONNECT_BASE + 13),
			USER_NOT_RESPOND = (AC_CONNECT_BASE + 14),

			// Ошибки валидации профилей
			//
			INVALID_PROFILE_SCHEMA = (PROFILE_BASE + 1),
			PROFILE_MISSING = (PROFILE_BASE + 2),
			INVALID_PROFILE_NAME = (PROFILE_BASE + 3),
			INVALID_PROFILE_TYPE = (PROFILE_BASE + 4),
			INVALID_PHY_TYPE = (PROFILE_BASE + 5),
			MSM_SECURITY_MISSING = (PROFILE_BASE + 6),
			IHV_SECURITY_NOT_SUPPORTED = (PROFILE_BASE + 7),
			IHV_OUI_MISMATCH = (PROFILE_BASE + 8),
			// IHV OUI не присутствует, но есть настройки IHV в профиле
			IHV_OUI_MISSING = (PROFILE_BASE + 9),
			// IHV OUI присутствует, но нету настроек IHV в профиле
			IHV_SETTINGS_MISSING = (PROFILE_BASE + 10),
			// оба/конфликт настроек MSMSec и IHV в профиле 
			CONFLICT_SECURITY = (PROFILE_BASE + 11),
			// нет настроек IHV или MSMSec в профиле
			SECURITY_MISSING = (PROFILE_BASE + 12),
			INVALID_BSS_TYPE = (PROFILE_BASE + 13),
			INVALID_ADHOC_CONNECTION_MODE = (PROFILE_BASE + 14),
			NON_BROADCAST_SET_FOR_ADHOC = (PROFILE_BASE + 15),
			AUTO_SWITCH_SET_FOR_ADHOC = (PROFILE_BASE + 16),
			AUTO_SWITCH_SET_FOR_MANUAL_CONNECTION = (PROFILE_BASE + 17),
			IHV_SECURITY_ONEX_MISSING = (PROFILE_BASE + 18),
			PROFILE_SSID_INVALID = (PROFILE_BASE + 19),
			TOO_MANY_SSID = (PROFILE_BASE + 20),

			// Причины несовместимости сети MSM
			//
			UNSUPPORTED_SECURITY_SET_BY_OS = (MSM_BASE + 1),
			UNSUPPORTED_SECURITY_SET = (MSM_BASE + 2),
			BSS_TYPE_UNMATCH = (MSM_BASE + 3),
			PHY_TYPE_UNMATCH = (MSM_BASE + 4),
			DATARATE_UNMATCH = (MSM_BASE + 5),

			// Причины ошибок подключения MSM
			// Коды причин ошибок
			//
			// Пользователь вызвал отключение
			USER_CANCELLED = (MSM_CONNECT_BASE + 1),
			// Отключение во время ассоциации
			ASSOCIATION_FAILURE = (MSM_CONNECT_BASE + 2),
			// Тайм-аут ассоциации
			ASSOCIATION_TIMEOUT = (MSM_CONNECT_BASE + 3),
			// Предварительная безопасность завершилась с ошибкой
			PRE_SECURITY_FAILURE = (MSM_CONNECT_BASE + 4),
			// Ошибка запуска безопасности после ассоциации
			START_SECURITY_FAILURE = (MSM_CONNECT_BASE + 5),
			// Безопасность после ассоциации завершилась с ошибкой
			SECURITY_FAILURE = (MSM_CONNECT_BASE + 6),
			// Тайм-аут сторожа безопасности
			SECURITY_TIMEOUT = (MSM_CONNECT_BASE + 7),
			// Отключение от драйвера при роуминге
			ROAMING_FAILURE = (MSM_CONNECT_BASE + 8),
			// Ошибка запуска безопасности при роуминге
			ROAMING_SECURITY_FAILURE = (MSM_CONNECT_BASE + 9),
			// Ошибка запуска безопасности для adhoc
			ADHOC_SECURITY_FAILURE = (MSM_CONNECT_BASE + 10),
			// Отключение от драйвера
			DRIVER_DISCONNECTED = (MSM_CONNECT_BASE + 11),
			// Операция драйвера не удалась
			DRIVER_OPERATION_FAILURE = (MSM_CONNECT_BASE + 12),
			// Служба IHV недоступна
			IHV_NOT_AVAILABLE = (MSM_CONNECT_BASE + 13),
			// Ответ от IHV истёк по времени
			IHV_NOT_RESPONDING = (MSM_CONNECT_BASE + 14),
			// Истекло время ожидания отключения драйвера
			DISCONNECT_TIMEOUT = (MSM_CONNECT_BASE + 15),
			// Внутренняя ошибка помешала завершению операции
			INTERNAL_FAILURE = (MSM_CONNECT_BASE + 16),
			// Тайм-аут запроса интерфейса
			UI_REQUEST_TIMEOUT = (MSM_CONNECT_BASE + 17),
			// Слишком частый роумминг, безопасность не завершена после 5 попыток
			TOO_MANY_SECURITY_ATTEMPTS = (MSM_CONNECT_BASE + 18),

			// Коды причин MSMSEC
			//

			MSMSEC_MIN = MSMSEC_BASE,

			// Указанный индекс ключа недействителен
			MSMSEC_PROFILE_INVALID_KEY_INDEX = (MSMSEC_BASE + 1),
			// Требуется ключ, PSK присутствует
			MSMSEC_PROFILE_PSK_PRESENT = (MSMSEC_BASE + 2),
			// Недействительная длина ключа
			MSMSEC_PROFILE_KEY_LENGTH = (MSMSEC_BASE + 3),
			// Недействительная длина PSK
			MSMSEC_PROFILE_PSK_LENGTH = (MSMSEC_BASE + 4),
			// Не указана аутентификация/шифр
			MSMSEC_PROFILE_NO_AUTH_CIPHER_SPECIFIED = (MSMSEC_BASE + 5),
			// Указано слишком много аутентификаций/шифров
			MSMSEC_PROFILE_TOO_MANY_AUTH_CIPHER_SPECIFIED = (MSMSEC_BASE + 6),
			// Профиль содержит дублирующуюся аутентификацию/шифр
			MSMSEC_PROFILE_DUPLICATE_AUTH_CIPHER = (MSMSEC_BASE + 7),
			// Необработанные данные профиля недействительны
			MSMSEC_PROFILE_RAWDATA_INVALID = (MSMSEC_BASE + 8),
			// Недействительная комбинация аутентификации/шифра
			MSMSEC_PROFILE_INVALID_AUTH_CIPHER = (MSMSEC_BASE + 9),
			// 802.1x отключен, когда требуется включение
			MSMSEC_PROFILE_ONEX_DISABLED = (MSMSEC_BASE + 10),
			// 802.1x включен, когда требуется отключение
			MSMSEC_PROFILE_ONEX_ENABLED = (MSMSEC_BASE + 11),
			MSMSEC_PROFILE_INVALID_PMKCACHE_MODE = (MSMSEC_BASE + 12),
			MSMSEC_PROFILE_INVALID_PMKCACHE_SIZE = (MSMSEC_BASE + 13),
			MSMSEC_PROFILE_INVALID_PMKCACHE_TTL = (MSMSEC_BASE + 14),
			MSMSEC_PROFILE_INVALID_PREAUTH_MODE = (MSMSEC_BASE + 15),
			MSMSEC_PROFILE_INVALID_PREAUTH_THROTTLE = (MSMSEC_BASE + 16),
			// PreAuth включен при отключенном кеше PMK
			MSMSEC_PROFILE_PREAUTH_ONLY_ENABLED = (MSMSEC_BASE + 17),
			// Сопоставление возможностей не удалось в сети
			MSMSEC_CAPABILITY_NETWORK = (MSMSEC_BASE + 18),
			// Сопоставление возможностей не удалось в сетевом адаптере
			MSMSEC_CAPABILITY_NIC = (MSMSEC_BASE + 19),
			// Сопоставление возможностей не удалось в профиле
			MSMSEC_CAPABILITY_PROFILE = (MSMSEC_BASE + 20),
			// Сеть не поддерживает указанный тип обнаружения
			MSMSEC_CAPABILITY_DISCOVERY = (MSMSEC_BASE + 21),
			// Парольная фраза содержит недействительный символ
			MSMSEC_PROFILE_PASSPHRASE_CHAR = (MSMSEC_BASE + 22),
			// Ключевой материал содержит недействительный символ
			MSMSEC_PROFILE_KEYMATERIAL_CHAR = (MSMSEC_BASE + 23),
			// Неправильный тип ключа для пары аутентификации/шифра
			MSMSEC_PROFILE_WRONG_KEYTYPE = (MSMSEC_BASE + 24),
			// Подозревается смешанная ячейка
			MSMSEC_MIXED_CELL = (MSMSEC_BASE + 25),
			// Таймеры аутентификации или кол-во тайм-аутов в профиле неправильно
			MSMSEC_PROFILE_AUTH_TIMERS_INVALID = (MSMSEC_BASE + 26),
			// Интервал обновления группового ключа в профиле неправильно
			MSMSEC_PROFILE_INVALID_GKEY_INTV = (MSMSEC_BASE + 27),
			// Подозревается переходная сеть, попытка наследственной безопасности
			MSMSEC_TRANSITION_NETWORK = (MSMSEC_BASE + 28),
			// Ключ содержит символы, которые не отображаются на ASCII
			MSMSEC_PROFILE_KEY_UNMAPPED_CHAR = (MSMSEC_BASE + 29),
			// Сопоставление возможностей не удалось в профиле (аутентификация)
			MSMSEC_CAPABILITY_PROFILE_AUTH = (MSMSEC_BASE + 30),
			// Сопоставление возможностей не удалось в профиле (шифр)
			MSMSEC_CAPABILITY_PROFILE_CIPHER = (MSMSEC_BASE + 31),

			// Ошибка постановки запроса интерфейса в очередь
			MSMSEC_UI_REQUEST_FAILURE = (MSMSEC_CONNECT_BASE + 1),
			// Аутентификация 802.1x не началась в установленное время 
			MSMSEC_AUTH_START_TIMEOUT = (MSMSEC_CONNECT_BASE + 2),
			// Аутентификация 802.1x не завершилась в установленное время
			MSMSEC_AUTH_SUCCESS_TIMEOUT = (MSMSEC_CONNECT_BASE + 3),
			// Динамический обмен ключами не начался в установленное время
			MSMSEC_KEY_START_TIMEOUT = (MSMSEC_CONNECT_BASE + 4),
			// Динамический обмен ключами не удался в установленное время
			MSMSEC_KEY_SUCCESS_TIMEOUT = (MSMSEC_CONNECT_BASE + 5),
			// Сообщение 3 четырёхстороннего рукопожатия не содержит данных
			MSMSEC_M3_MISSING_KEY_DATA = (MSMSEC_CONNECT_BASE + 6),
			// Сообщение 3 четырёхстороннего рукопожатия не содержит IE
			MSMSEC_M3_MISSING_IE = (MSMSEC_CONNECT_BASE + 7),
			// Сообщение 3 не содержит группового ключа
			MSMSEC_M3_MISSING_GRP_KEY = (MSMSEC_CONNECT_BASE + 8),
			// Сопоставление возможностей IE в M3 не удалось
			MSMSEC_PR_IE_MATCHING = (MSMSEC_CONNECT_BASE + 9),
			// Сопоставление возможностей вторичного IE в M3 не удалось
			MSMSEC_SEC_IE_MATCHING = (MSMSEC_CONNECT_BASE + 10),
			// Требуется парный ключ, но точка доступа настроена на групповые
			MSMSEC_NO_PAIRWISE_KEY = (MSMSEC_CONNECT_BASE + 11),
			// Сообщение 1 рукопожатия группового ключа не содержит данных
			MSMSEC_G1_MISSING_KEY_DATA = (MSMSEC_CONNECT_BASE + 12),
			// Сообщение 1 не содержит группового ключа
			MSMSEC_G1_MISSING_GRP_KEY = (MSMSEC_CONNECT_BASE + 13),
			// Точка доступа сбросила защищенный бит после соединения
			MSMSEC_PEER_INDICATED_INSECURE = (MSMSEC_CONNECT_BASE + 14),
			// 802.1x указывает на отсутствие аутентификатора, но требует
			MSMSEC_NO_AUTHENTICATOR = (MSMSEC_CONNECT_BASE + 15),
			// Ошибка применения параметров к сетевому адаптеру
			MSMSEC_NIC_FAILURE = (MSMSEC_CONNECT_BASE + 16),
			// Операция отменена вызывающей стороной
			MSMSEC_CANCELLED = (MSMSEC_CONNECT_BASE + 17),
			// Ключ имел неправильный формат 
			MSMSEC_KEY_FORMAT = (MSMSEC_CONNECT_BASE + 18),
			// Обнаружено понижение уровня безопасности
			MSMSEC_DOWNGRADE_DETECTED = (MSMSEC_CONNECT_BASE + 19),
			// Подозревается несовпадение PSK
			MSMSEC_PSK_MISMATCH_SUSPECTED = (MSMSEC_CONNECT_BASE + 20),
			// Принудительный отказ из-за небезопасного метода подключения
			MSMSEC_FORCED_FAILURE = (MSMSEC_CONNECT_BASE + 21),
			// Не удалось поставить запросу интерфейса или пользователь нажал отмену
			MSMSEC_SECURITY_UI_FAILURE = (MSMSEC_CONNECT_BASE + 22),

			MSMSEC_MAX = MSMSEC_END
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WlanConnectionNotificationData
		{
			public WlanConnectionMode wlanConnectionMode;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string profileName;
			public Dot11Ssid dot11Ssid;
			public Dot11BssType dot11BssType;
			public bool securityEnabled;
			public WlanReasonCode wlanReasonCode;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
			public string profileXml;
		}

		public enum WlanInterfaceState
		{
			NotReady = 0,
			Connected = 1,
			AdHocNetworkFormed = 2,
			Disconnecting = 3,
			Disconnected = 4,
			Associating = 5,
			Discovering = 6,
			Authenticating = 7
		}

		public struct Dot11Ssid
		{
			public uint SSIDLength;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] SSID;
		}

		public enum Dot11PhyType : uint
		{
			Unknown = 0,
			Any = Unknown,
			FHSS = 1,
			DSSS = 2,
			IrBaseband = 3,
			OFDM = 4,
			HRDSSS = 5,
			ERP = 6,
			IHV_Start = 0x80000000,
			IHV_End = 0xffffffff
		}

		public enum Dot11BssType
		{
			Infrastructure = 1,
			Independent = 2,
			Any = 3
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WlanAssociationAttributes
		{
			public Dot11Ssid dot11Ssid;
			public Dot11BssType dot11BssType;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] dot11Bssid;
			public Dot11PhyType dot11PhyType;
			public uint dot11PhyIndex;
			public uint wlanSignalQuality;
			public uint rxRate;
			public uint txRate;

			public PhysicalAddress Dot11Bssid
			{
				get { return new PhysicalAddress(dot11Bssid); }
			}
		}

		public enum WlanConnectionMode
		{
			Profile = 0,
			TemporaryProfile,
			DiscoverySecure,
			DiscoveryUnsecure,
			Auto,
			Invalid
		}

		public enum Dot11AuthAlgorithm : uint
		{
			IEEE80211_Open = 1,
			IEEE80211_SharedKey = 2,
			WPA = 3,
			WPA_PSK = 4,
			WPA_None = 5,
			RSNA = 6,
			RSNA_PSK = 7,
			IHV_Start = 0x80000000,
			IHV_End = 0xffffffff
		}

		public enum Dot11CipherAlgorithm : uint
		{
			None = 0x00,
			WEP40 = 0x01,
			TKIP = 0x02,
			CCMP = 0x04,
			WEP104 = 0x05,
			WPA_UseGroup = 0x100,
			RSN_UseGroup = 0x100,
			WEP = 0x101,
			IHV_Start = 0x80000000,
			IHV_End = 0xffffffff
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WlanSecurityAttributes
		{
			[MarshalAs(UnmanagedType.Bool)]
			public bool securityEnabled;
			[MarshalAs(UnmanagedType.Bool)]
			public bool oneXEnabled;
			public Dot11AuthAlgorithm dot11AuthAlgorithm;
			public Dot11CipherAlgorithm dot11CipherAlgorithm;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WlanConnectionAttributes
		{
			public WlanInterfaceState isState;
			public WlanConnectionMode wlanConnectionMode;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string profileName;
			public WlanAssociationAttributes wlanAssociationAttributes;
			public WlanSecurityAttributes wlanSecurityAttributes;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WlanInterfaceInfo
		{
			public Guid interfaceGuid;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string interfaceDescription;
			public WlanInterfaceState isState;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct WlanInterfaceInfoListHeader
		{
			public uint numberOfItems;
			public uint index;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct WlanProfileInfoListHeader
		{
			public uint numberOfItems;
			public uint index;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WlanProfileInfo
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string profileName;
			public WlanProfileFlags profileFlags;
		}

		[Flags]
		public enum Dot11OperationMode : uint
		{
			Unknown = 0x00000000,
			Station = 0x00000001,
			AP = 0x00000002,
			ExtensibleStation = 0x00000004,
			NetworkMonitor = 0x80000000
		}

		public enum Dot11RadioState : uint
		{
			Unknown = 0,
			On,
			Off
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WlanPhyRadioState
		{
			public int dwPhyIndex;
			public Dot11RadioState dot11SoftwareRadioState;
			public Dot11RadioState dot11HardwareRadioState;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WlanRadioState
		{
			public int numberofItems;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
			private WlanPhyRadioState[] phyRadioState;
			public WlanPhyRadioState[] PhyRadioState
			{
				get
				{
					WlanPhyRadioState[] ret = new WlanPhyRadioState[numberofItems];
					Array.Copy(phyRadioState, ret, numberofItems);
					return ret;
				}
			}
		}

		[DebuggerStepThrough]
		internal static void ThrowIfError(int win32ErrorCode)
		{
			if (win32ErrorCode != 0)
				throw new Win32Exception(win32ErrorCode);
		}
	}
}
