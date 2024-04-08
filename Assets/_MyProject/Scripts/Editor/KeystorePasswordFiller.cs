using UnityEditor;

[InitializeOnLoad]

public class KeystorePasswordFiller
{
    static KeystorePasswordFiller()
    {
        UnityEditor.PlayerSettings.keystorePass = "Kjkszpj123";
        UnityEditor.PlayerSettings.keyaliasPass = "Kjkszpj123";
    }
}
