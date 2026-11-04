using System.IO;
using System.Runtime.InteropServices;
using SimpleScan.Core.Models;

#pragma warning disable CS8600, CS8602, CS8604
namespace SimpleScan.Core.Scanning;

public class WiaScannerService : IScannerService
{
    private const string WiaFormatBmp = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";
    private const string WiaFormatPng = "{B96B3CAF-0728-11D3-9D7B-0000F81EF32E}";
    private const string WiaFormatJpg = "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}";

    // WIA Device Info Property IDs
    private const int WiaDevPropIdManufacturer = 3;
    private const int WiaDevPropIdDescription = 4;
    private const int WiaDevPropIdType = 5;
    private const int WiaDevPropIdName = 7;

    // WIA Item Property IDs
    private const int WiaPropIdCurrentIntent = 6146;
    private const int WiaPropIdHorizontalResolution = 6147;
    private const int WiaPropIdVerticalResolution = 6148;
    private const int WiaPropIdHorizontalExtent = 6151;
    private const int WiaPropIdVerticalExtent = 6152;
    private const int WiaPropIdBrightness = 6154;
    private const int WiaPropIdContrast = 6155;

    public Task<IReadOnlyList<ScannerDevice>> GetDevicesAsync()
    {
        return RunInStaThread(() =>
        {
            var list = new List<ScannerDevice>();
            Type? devMgrType = Type.GetTypeFromProgID("WIA.DeviceManager");
            if (devMgrType == null)
            {
                return (IReadOnlyList<ScannerDevice>)list;
            }

            dynamic? manager = Activator.CreateInstance(devMgrType);
            if (manager == null) return list;

            try
            {
                dynamic deviceInfos = manager.DeviceInfos;
                int count = deviceInfos.Count;

                for (int i = 1; i <= count; i++)
                {
                    try
                    {
                        dynamic info = deviceInfos[i];
                        string id = Convert.ToString(info.DeviceID) ?? string.Empty;

                        int type = 0;
                        try { type = Convert.ToInt32(info.Type); } catch { }

                        // In WIA: WiaDeviceType.ScannerDeviceType is 1 (or 65537 in raw STI properties)
                        bool isScanner = type == 1 || type == 65537 ||
                                         id.Contains("Scan", StringComparison.OrdinalIgnoreCase) ||
                                         id.Contains("Escl", StringComparison.OrdinalIgnoreCase);

                        if (!isScanner)
                        {
                            continue;
                        }

                        string name = GetDevicePropertyById(info.Properties, WiaDevPropIdName, "Escáner WIA");
                        string desc = GetDevicePropertyById(info.Properties, WiaDevPropIdDescription, name);
                        string manufacturer = GetDevicePropertyById(info.Properties, WiaDevPropIdManufacturer, "Genérico");

                        bool isHp = name.Contains("HP", StringComparison.OrdinalIgnoreCase) ||
                                    desc.Contains("HP", StringComparison.OrdinalIgnoreCase) ||
                                    id.Contains("HP", StringComparison.OrdinalIgnoreCase);

                        list.Add(new ScannerDevice(id, name, manufacturer, desc, isHp));
                    }
                    catch
                    {
                        // Ignore devices that fail enumeration
                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(manager);
            }

            // Order so that HP Smart Tank or preferred devices come first
            return (IReadOnlyList<ScannerDevice>)list
                .OrderByDescending(d => d.IsDefault)
                .ThenBy(d => d.Name)
                .ToList();
        });
    }

    public Task<byte[]> ScanPageAsync(
        string? deviceId,
        ScanSettings settings,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return RunInStaThread(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report("Conectando con el escáner...");

            Type? devMgrType = Type.GetTypeFromProgID("WIA.DeviceManager");
            if (devMgrType == null)
            {
                throw new InvalidOperationException("El subsistema WIA no está disponible en este equipo Windows.");
            }

            dynamic? manager = Activator.CreateInstance(devMgrType);
            if (manager == null)
            {
                throw new InvalidOperationException("No se pudo iniciar el gestor de dispositivos WIA.");
            }

            dynamic? device = null;
            try
            {
                dynamic deviceInfos = manager.DeviceInfos;
                dynamic? targetInfo = null;

                if (!string.IsNullOrEmpty(deviceId))
                {
                    for (int i = 1; i <= deviceInfos.Count; i++)
                    {
                        if (string.Equals(Convert.ToString(deviceInfos[i].DeviceID), deviceId, StringComparison.OrdinalIgnoreCase))
                        {
                            targetInfo = deviceInfos[i];
                            break;
                        }
                    }
                }

                // If not found by ID, pick the first scanner
                if (targetInfo == null && deviceInfos.Count > 0)
                {
                    targetInfo = deviceInfos[1];
                }

                if (targetInfo == null)
                {
                    throw new FileNotFoundException("No se encontró ningún escáner conectado. Asegúrate de que la impresora está encendida y conectada a la red Wi-Fi o por cable USB.");
                }

                string devName = GetDevicePropertyById(targetInfo.Properties, WiaDevPropIdName, "Escáner");
                progress?.Report($"Conectado a {devName}. Configurando resolución ({settings.Dpi} DPI)...");

                device = targetInfo.Connect();
                if (device == null || ((dynamic)device).Items.Count == 0)
                {
                    throw new InvalidOperationException("El dispositivo está conectado pero no ofrece elementos de escaneo disponibles.");
                }

                dynamic item = ((dynamic)device).Items[1];

                // Configure DPI
                SetItemProperty(item.Properties, WiaPropIdHorizontalResolution, settings.Dpi);
                SetItemProperty(item.Properties, WiaPropIdVerticalResolution, settings.Dpi);

                // Configure Color Intent (1 = Color, 2 = Grayscale, 4 = Text/B&W)
                SetItemProperty(item.Properties, WiaPropIdCurrentIntent, (int)settings.ColorMode);

                // Configure Brightness and Contrast if changed from 0
                if (settings.Brightness != 0)
                {
                    SetItemProperty(item.Properties, WiaPropIdBrightness, settings.Brightness);
                }
                if (settings.Contrast != 0)
                {
                    SetItemProperty(item.Properties, WiaPropIdContrast, settings.Contrast);
                }

                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report("Escaneando página... Espere por favor.");

                // Transfer image
                dynamic? imageFile = null;
                try
                {
                    // Attempt transfer in BMP format
                    imageFile = item.Transfer(WiaFormatBmp);
                }
                catch (COMException cx)
                {
                    // Fallback to default transfer format
                    try
                    {
                        imageFile = item.Transfer();
                    }
                    catch
                    {
                        throw InterpretComException(cx, devName);
                    }
                }

                if (imageFile == null)
                {
                    throw new InvalidOperationException("El escáner finalizó el proceso sin devolver datos de imagen.");
                }

                progress?.Report("Procesando imagen escaneada...");
                byte[] imageBytes = ExtractBytesFromImageFile(imageFile);
                return imageBytes;
            }
            finally
            {
                if (device != null) Marshal.ReleaseComObject(device);
                Marshal.ReleaseComObject(manager);
            }
        });
    }

    private static byte[] ExtractBytesFromImageFile(dynamic imageFile)
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"simplescan_{Guid.NewGuid():N}.tmp");
        try
        {
            imageFile.SaveFile(tempFile);
            return File.ReadAllBytes(tempFile);
        }
        catch
        {
            try
            {
                dynamic fileData = imageFile.FileData;
                return (byte[])fileData.BinaryData;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron extraer los bytes de la imagen escaneada: " + ex.Message, ex);
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                try { File.Delete(tempFile); } catch { }
            }
            try { Marshal.ReleaseComObject(imageFile); } catch { }
        }
    }

    private static void SetItemProperty(dynamic properties, int propertyId, object value)
    {
        try
        {
            foreach (dynamic prop in properties)
            {
                if (prop.PropertyID == propertyId)
                {
                    prop.Value = value;
                    break;
                }
            }
        }
        catch
        {
            // Some drivers report properties as read-only; ignore and proceed
        }
    }

    private static string GetDevicePropertyById(dynamic properties, int propertyId, string defaultValue)
    {
        try
        {
            foreach (dynamic prop in properties)
            {
                if (prop.PropertyID == propertyId)
                {
                    return Convert.ToString(prop.Value) ?? defaultValue;
                }
            }
        }
        catch { }
        return defaultValue;
    }

    private static Exception InterpretComException(COMException cx, string scannerName)
    {
        uint hr = (uint)cx.ErrorCode;
        return hr switch
        {
            0x80210006 => new InvalidOperationException($"El escáner '{scannerName}' está ocupado en otra tarea. Inténtalo de nuevo en unos segundos."),
            0x80210003 => new InvalidOperationException($"El escáner '{scannerName}' no responde o está apagado. Verifica la conexión."),
            0x80210001 => new InvalidOperationException($"Error general en el controlador de '{scannerName}'."),
            0x80210005 => new InvalidOperationException($"La tapa del escáner está abierta o falta papel en el alimentador."),
            0x80210007 => new OperationCanceledException("El escaneo fue cancelado por el usuario."),
            _ => new InvalidOperationException($"Error de comunicación con el escáner (0x{hr:X8}): {cx.Message}", cx)
        };
    }

    private static Task<T> RunInStaThread<T>(Func<T> func)
    {
        var tcs = new TaskCompletionSource<T>();
        var thread = new Thread(() =>
        {
            try
            {
                tcs.SetResult(func());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return tcs.Task;
    }
}
