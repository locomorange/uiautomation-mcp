namespace UIAutomationMCP.Shared.Validation
{
    /// <summary>
    /// Type-safe validation result
    /// </summary>
    public readonly record struct ValidationResult
    {
        public bool IsValid { get; init; }
        public string[] Errors { get; init; }
        
        public static ValidationResult Success => new(true, Array.Empty<string>());
        
        public static ValidationResult Failure(params string[] errors) => new(false, errors);
        
        public static ValidationResult Failure(IEnumerable<string> errors) => new(false, errors.ToArray());
        
        private ValidationResult(bool isValid, string[] errors)
        {
            IsValid = isValid;
            Errors = errors;
        }
        
        public ValidationResult Combine(ValidationResult other)
        {
            if (IsValid && other.IsValid)
                return Success;
            
            return new ValidationResult(false, Errors.Concat(other.Errors).ToArray());
        }
    }
}