using UnityEditor;

[InitializeOnLoad]

public class KeystorePasswordFiller
{
    static KeystorePasswordFiller()
    {
        PlayerSettings.keystorePass = "Kjkszpj123";
        PlayerSettings.keyaliasPass = "Kjkszpj123";
    }
}
