using SoundProfiler2.Models;
using SoundProfiler2.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vannatech.CoreAudio.Constants;
using Vannatech.CoreAudio.Enumerations;
using Vannatech.CoreAudio.Externals;
using Vannatech.CoreAudio.Interfaces;

namespace SoundProfiler2 {
    public class CoreAudioWrapper {
        #region Public Methods
        public static string[] GetOutputDeviceNames() {
            return GetDevices(isInput: false).Select(device => GetFriendlyName(device)).ToArray();
        }

        public static string[] GetIntputDeviceNames() {
            return GetDevices(isInput: true).Select(device => GetFriendlyName(device)).ToArray();
        }

        public static Dictionary<string, string[]> GetAudioSessionNames() {
            Dictionary<string, string[]> allSessionNames = new();
            foreach (IMMDevice device in GetDevices()) {
                List<string> sessionNames = new();
                foreach (IAudioSessionControl2 session in GetSessions(device)) {
                    session.GetDisplayName(out string displayName);
                    session.GetProcessId(out uint pid);
                    var process = Process.GetProcessById((int)pid);

                    if (!string.IsNullOrWhiteSpace(displayName)) {
                        sessionNames.Add(displayName);
                    } else {
                        if (!string.IsNullOrWhiteSpace(process.MainWindowTitle)) {
                            sessionNames.Add(process.MainWindowTitle);
                        } else {
                            sessionNames.Add(process.ProcessName);
                        }
                    }
                }

                allSessionNames.Add(GetFriendlyName(device), sessionNames.ToArray());
            }

            return allSessionNames;
        }

        public static MixerApplicationModel[] GetMixerApplications() {
            List<MixerApplicationModel> mixerApplications = new();

            foreach (IMMDevice device in GetDevices()) {
                foreach (IAudioSessionControl2 session in GetSessions(device)) {
                    session.GetDisplayName(out string displayName);
                    session.GetProcessId(out uint pid);

                    var process = Process.GetProcessById((int)pid);

                    string friendlyName;
                    if (!string.IsNullOrWhiteSpace(displayName)) {
                        friendlyName = displayName;
                    } else {
                        if (!string.IsNullOrWhiteSpace(process.MainWindowTitle)) {
                            friendlyName = process.MainWindowTitle;
                        } else {
                            friendlyName = process.ProcessName;
                        }
                    }

                    Icon icon;
                    try {
                        icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
                    } catch (Win32Exception) {
                        icon = Resources.Win32Project_16x_ico;
                    }

                    mixerApplications.Add(new MixerApplicationModel() {
                        DeviceName = GetFriendlyName(device),
                        ApplicationName = friendlyName,
                        ApplicationIcon = icon,
                        VolumeLevel = 0
                    });
                }
            }

            return mixerApplications.ToArray(); ;
        }
        #endregion Public Methods

        #region Private Methods
        private static IAudioSessionControl2[] GetSessions(IMMDevice device) {
            device.Activate(new Guid(ComIIDs.IAudioSessionManager2IID), (uint)CLSCTX.CLSCTX_INPROC_SERVER, IntPtr.Zero, out object objInterface);
            var sessionManager2 = (IAudioSessionManager2)objInterface;

            sessionManager2.GetSessionEnumerator(out IAudioSessionEnumerator sessionsEnumerator);

            List<IAudioSessionControl2> sessions = new();
            sessionsEnumerator.GetCount(out int sessionsCnt);
            for (int ii = 0; ii < sessionsCnt; ii++) {

                sessionsEnumerator.GetSession(ii, out IAudioSessionControl session);
                sessions.Add((IAudioSessionControl2)session);
            }

            return sessions.ToArray();
        }

        private static IMMDevice[] GetDevices(bool isInput = false) {
            var deviceEnumeratorType = Type.GetTypeFromCLSID(new Guid(ComCLSIDs.MMDeviceEnumeratorCLSID));
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)Activator.CreateInstance(deviceEnumeratorType);

            deviceEnumerator.EnumAudioEndpoints(isInput ? EDataFlow.eCapture : EDataFlow.eRender, DEVICE_STATE_XXX.DEVICE_STATE_ACTIVE, out IMMDeviceCollection deviceCollection);

            List<IMMDevice> devices = new();
            deviceCollection.GetCount(out uint devicesCnt);
            for (uint ii = 0; ii < devicesCnt; ii++) {
                deviceCollection.Item(ii, out IMMDevice device);
                devices.Add(device);
            }

            return devices.ToArray();
        }

        private static string GetFriendlyName(IMMDevice device) {
            device.OpenPropertyStore(STGM.STGM_READ, out IPropertyStore properties);

            properties.GetCount(out uint propertiesCnt);
            for (uint jj = 0; jj < propertiesCnt; jj++) {
                properties.GetAt(jj, out PROPERTYKEY key);
                if (key.fmtid == PropertyKeys.PKEY_DeviceInterface_FriendlyName) {
                    properties.GetValue(ref key, out PROPVARIANT value);
                    if ((VarEnum)value.vt == VarEnum.VT_LPWSTR) {
                        return Marshal.PtrToStringUni(value.Data.AsStringPtr);
                    } else {
                        /* Unexpected type */
                        throw new NotImplementedException("Unexpected type");
                    }
                }
            }

            /* None found */
            return null;
        }
        #endregion Private Methods
    }
}
