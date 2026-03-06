using PasswordManager.Core.Services;

namespace PasswordManager.Maui.Services;

public class MauiBiometricService : IBiometricService
{
    private const string MasterKeyStorageKey = "PasswordManager_MasterKey";
    private const string BiometricEnabledKey = "PasswordManager_BiometricEnabled";

    public Task<bool> IsAvailableAsync()
    {
#if ANDROID
        try
        {
            var context = Android.App.Application.Context;
            var biometricManager = AndroidX.Biometric.BiometricManager.From(context);
            var canAuthenticate = biometricManager.CanAuthenticate(
                AndroidX.Biometric.BiometricManager.Authenticators.BiometricStrong |
                AndroidX.Biometric.BiometricManager.Authenticators.BiometricWeak);
            return Task.FromResult(canAuthenticate == AndroidX.Biometric.BiometricManager.BiometricSuccess);
        }
        catch
        {
            return Task.FromResult(false);
        }
#elif IOS || MACCATALYST
        try
        {
            var context = new LocalAuthentication.LAContext();
            return Task.FromResult(context.CanEvaluatePolicy(
                LocalAuthentication.LAPolicy.DeviceOwnerAuthenticationWithBiometrics,
                out _));
        }
        catch
        {
            return Task.FromResult(false);
        }
#else
        return Task.FromResult(false);
#endif
    }

    public async Task<bool> AuthenticateAsync(string reason)
    {
#if ANDROID
        try
        {
            var activity = Platform.CurrentActivity;
            if (activity == null) return false;

            var tcs = new TaskCompletionSource<bool>();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var fragmentActivity = activity as AndroidX.Fragment.App.FragmentActivity;
                    if (fragmentActivity == null)
                    {
                        tcs.TrySetResult(false);
                        return;
                    }

                    var executor = AndroidX.Core.Content.ContextCompat.GetMainExecutor(activity);
                    var callback = new BiometricCallback(tcs);

                    var biometricPrompt = new AndroidX.Biometric.BiometricPrompt(
                        fragmentActivity,
                        executor,
                        callback);

                    var promptInfo = new AndroidX.Biometric.BiometricPrompt.PromptInfo.Builder()
                        .SetTitle("Password Manager")
                        .SetSubtitle(reason)
                        .SetNegativeButtonText("Cancel")
                        .Build();

                    biometricPrompt.Authenticate(promptInfo);
                }
                catch (Exception)
                {
                    tcs.TrySetResult(false);
                }
            });

            return await tcs.Task;
        }
        catch
        {
            return false;
        }
#elif IOS || MACCATALYST
        try
        {
            var context = new LocalAuthentication.LAContext();
            var canEvaluate = context.CanEvaluatePolicy(
                LocalAuthentication.LAPolicy.DeviceOwnerAuthenticationWithBiometrics,
                out _);

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
        await Task.CompletedTask;
        return false;
#endif
    }

    public async Task<bool> IsEnabledAsync()
    {
        try
        {
            var value = await SecureStorage.Default.GetAsync(BiometricEnabledKey);
            return value == "true";
        }
        catch
        {
            return false;
        }
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        try
        {
            if (enabled)
            {
                await SecureStorage.Default.SetAsync(BiometricEnabledKey, "true");
            }
            else
            {
                SecureStorage.Default.Remove(BiometricEnabledKey);
            }
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public async Task StoreMasterKeyAsync(string key)
    {
        try
        {
            await SecureStorage.Default.SetAsync(MasterKeyStorageKey, key);
        }
        catch
        {
            // Ignore storage errors
        }
    }

    public async Task<string?> RetrieveMasterKeyAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(MasterKeyStorageKey);
        }
        catch
        {
            return null;
        }
    }

    public Task ClearStoredKeyAsync()
    {
        try
        {
            SecureStorage.Default.Remove(MasterKeyStorageKey);
            SecureStorage.Default.Remove(BiometricEnabledKey);
        }
        catch
        {
            // Ignore storage errors
        }
        return Task.CompletedTask;
    }

#if ANDROID
    private class BiometricCallback : AndroidX.Biometric.BiometricPrompt.AuthenticationCallback
    {
        private readonly TaskCompletionSource<bool> _tcs;

        public BiometricCallback(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
        }

        public override void OnAuthenticationSucceeded(AndroidX.Biometric.BiometricPrompt.AuthenticationResult result)
        {
            base.OnAuthenticationSucceeded(result);
            _tcs.TrySetResult(true);
        }

        public override void OnAuthenticationFailed()
        {
            base.OnAuthenticationFailed();
        }

        public override void OnAuthenticationError(int errorCode, Java.Lang.ICharSequence? errString)
        {
            base.OnAuthenticationError(errorCode, errString);
            _tcs.TrySetResult(false);
        }
    }
#endif
}
