-Inside Firebase console/authentication/sign-in method 
	enable anonymous and google sign in
-Firebase console/project overview/project settings/your apps
    add both SHA fingerprints for your app (keytool -exportcert -alias [ALIAS_NAME]-keystore [KEYSTORE_PATH] -list -v)
	export google-services.json
	add (by putting it somewhere inside Assets folder)
        (or replace existing) google-services.json file inside Unity project
-Under Google sign in option/Web SDK configuration, copy the Web client ID
	Inside GoogleSignInHandler.cs replace WEB_CLIENT_ID

