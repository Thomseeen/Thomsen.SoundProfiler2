using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vannatech.CoreAudio.Constants;
using Vannatech.CoreAudio.Externals;
using Vannatech.CoreAudio.Interfaces;

namespace SoundProfiler2 {
    public class CoreAudioWrapper {
        public static string[] GetDeviceNames() {
            var deviceEnumeratorType = Type.GetTypeFromCLSID(new Guid(ComCLSIDs.MMDeviceEnumeratorCLSID));
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)Activator.CreateInstance(deviceEnumeratorType);

            deviceEnumerator.EnumAudioEndpoints(Vannatech.CoreAudio.Enumerations.EDataFlow.eRender, DEVICE_STATE_XXX.DEVICE_STATE_ACTIVE, out IMMDeviceCollection devices);

            Dictionary<string, string> names = new();
            devices.GetCount(out uint devicesCnt);
            for (uint ii = 0; ii < devicesCnt; ii++) {
                devices.Item(ii, out IMMDevice device);
                device.GetId(out string id);
                device.OpenPropertyStore(STGM.STGM_READ, out IPropertyStore properties);

                properties.GetCount(out uint propertiesCnt);
                for (uint jj = 0; jj < propertiesCnt; jj++) {
                    properties.GetAt(jj, out PROPERTYKEY key);
                    if (key.fmtid == PropertyKeys.PKEY_DeviceInterface_FriendlyName) {
                        properties.GetValue(ref key, out PROPVARIANT value);
                        if ((VarEnum)value.vt == VarEnum.VT_LPWSTR) {
                            names.TryAdd(id, Marshal.PtrToStringUni(value.Data.AsStringPtr));
                        }
                    }
                }
            }

            return names.Values.ToArray();
        }
    }
}
