$content = Get-Content -Raw "C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\BeachRehberi.API\BeachRehberi.API\Controllers\AuthController.cs"

$newConstructor = @"
        private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
        private readonly IValidator<VerifyEmailRequest> _verifyEmailValidator;
        private readonly IValidator<ResendVerificationRequest> _resendVerificationValidator;

        public AuthController(
            IAuthService authService, 
            IConfiguration configuration, 
            IWebHostEnvironment env,
            IValidator<ForgotPasswordRequest> forgotPasswordValidator,
            IValidator<ResetPasswordRequest> resetPasswordValidator,
            IValidator<VerifyEmailRequest> verifyEmailValidator,
            IValidator<ResendVerificationRequest> resendVerificationValidator)
        {
            _authService = authService;
            _configuration = configuration;
            _env = env;
            _forgotPasswordValidator = forgotPasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _verifyEmailValidator = verifyEmailValidator;
            _resendVerificationValidator = resendVerificationValidator;
        }
"@

$content = $content -replace '(?s)public AuthController\(IAuthService.*?\}', $newConstructor

$newForgotPassword = @"
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var validation = await _forgotPasswordValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

            var result = await _authService.ForgotPasswordAsync(request);
            return Ok(result); // Her zaman 200 dön (güvenlik)
        }
"@
$content = $content -replace '(?s)\[AllowAnonymous\]\s*\[HttpPost\("forgot-password"\)\].*?return result.*?\}', $newForgotPassword


$newResetPassword = @"
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var validation = await _resetPasswordValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

            var result = await _authService.ResetPasswordAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
"@
$content = $content -replace '(?s)\[AllowAnonymous\]\s*\[HttpPost\("reset-password"\)\].*?return result.*?\}', $newResetPassword


$newVerifyEmail = @"
        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var validation = await _verifyEmailValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

            var result = await _authService.VerifyEmailAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
"@
$content = $content -replace '(?s)\[AllowAnonymous\]\s*\[HttpPost\("verify-email"\)\].*?return result.*?\}', $newVerifyEmail


$newResendVerification = @"
        [AllowAnonymous]
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            var validation = await _resendVerificationValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

            var result = await _authService.ResendVerificationAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }
"@
$content = $content -replace '(?s)\[AllowAnonymous\]\s*\[HttpPost\("resend-verification"\)\].*?return result.*?\}', $newResendVerification

Set-Content -Path "C:\Users\akMuratNET\Desktop\BeachGo\BeachGo-ReactWeb\BeachRehberi.API\BeachRehberi.API\Controllers\AuthController.cs" -Value $content
