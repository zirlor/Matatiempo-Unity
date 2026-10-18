using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SetKeystorePasswords
{
    static SetKeystorePasswords()
    {
        // Solo establecer si no están ya definidas
        if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass))
        {
            PlayerSettings.Android.keystorePass = "Derowin.22";
        }
        if (string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
        {
            PlayerSettings.Android.keyaliasPass = "Derowin.22";
        }
        PlayerSettings.Android.useCustomKeystore = true;
        // Opcional: asegurar ruta del keystore
        // PlayerSettings.Android.keystoreName = "C:/Users/zirow/source/repos/IMRtecnologygroup/polixgamestudio/keystore/matatiempo-release.keystore";
    }
}
