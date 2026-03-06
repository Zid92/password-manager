using PasswordManager.Core.Services;

namespace PasswordManager.Maui.Services;

public class MauiBiometricService : IBiometricService
{
    private const string MasterKeyStorageKey = "PasswordManager_MasterKey";

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync("__biometric_check__") != null || true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AuthenticateAsync(string reason)
    {
#if ANDROID
        try
        {
            var context = Platform.CurrentActivity;
            if (context == null) return false;

            var executor = AndroidX.Core.Content.ContextCompat.GetMainExecutor(context);
            var biometricPrompt = new AndroidX.Biometric.BiometricPrompt(
                (AndroidX.Fragment.App.FragmentActivity)context,
                executor,
                new BiometricAuthCallback());

            var promptInfo = new AndroidX.Biometric.BiometricPrompt.PromptInfo.Builder()
                .SetTitle("Unlock Password Manager")
                .SetSubtitle(reason)
                .SetNegativeButtonText("Cancel")
                .Build();

            var tcs = new TaskCompletionSource<bool>();
            biometricPrompt.Authenticate(promptInfo);
            return await tcs.Task;
        }
        catch
        {
            return false;
        }
#elif IOS
        try
        {
            var context = new LocalAuthentication.LAContext();
            var canEvaluate = context.CanEvaluatePolicy(
                LocalAuthentication.LAPolicy.DeviceOwnerAuthenticationWithBiometrics,
                out var error);

            if (!canEvaluate) return false;

            var (success, _) = await context.EvaluatePolicyAsync(
                LocalAuthentication.LAPolicy.DeviceOwnerAuthenticationWithBiometrics,
                reason);

            return success;
        }
        catch
        {
            return false;
        }
#else
        return await Task.FromResult(false);
#endif
    }

    public async Task<bool> IsEnabledAsync()
    {
        var value = await SecureStorage.Default.GetAsync($"{MasterKeyStorageKey}_enabled");
        return value == "true";
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        if (enabled)
        {
            await SecureStorage.Default.SetAsync($"{MasterKeyStorageKey}_enabled", "true");
        }
        else
        {
            SecureStorage.Default.Remove($"{MasterKeyStorageKey}_enabled");
        }
    }

    public async Task StoreMasterKeyAsync(string key)
    {
        await SecureStorage.Default.SetAsync(MasterKeyStorageKey, key);
    }

    public async Task<string?> RetrieveMasterKeyAsync()
    {
        return await SecureStorage.Default.GetAsync(MasterKeyStorageKey);
    }

    public Task ClearStoredKeyAsync()
    {
        SecureStorage.Default.Remove(MasterKeyStorageKey);
        SecureStorage.Default.Remove($"{MasterKeyStorageKey}_enabled");
        return Task.CompletedTask;
    }

#if ANDROID
    private class BiometricAuthCallback : AndroidX.Biometric.BiometricPrompt.AuthenticationCallback
    {
        public TaskCompletionSource<bool>? TaskSource { get; set; }

        public override void OnAuthenticationSucceeded(AndroidX.Biometric.BiometricPrompt.AuthenticationResult result)
        {
            base.OnAuthenticationSucceeded(result);
            TaskSource?.TrySetResult(true);
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();
        }

        public override void OnAuthenticationError(int errorCode, Java.Lang.ICharSequence errString)
        {
            base.OnAuthenticationError(errorCode, errString);
            TaskSource?.TrySetResult(false);
        }
    }
#endif
}
