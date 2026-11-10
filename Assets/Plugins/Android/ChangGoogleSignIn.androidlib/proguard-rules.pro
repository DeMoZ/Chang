# called from C# (GoogleAndroidSignIn.cs) by name
-keep class com.roman.kolychev.chang.googlesignin.** { *; }
# Credential Manager loads the Play services provider by reflection
-if class androidx.credentials.CredentialManager
-keep class androidx.credentials.playservices.** { *; }
