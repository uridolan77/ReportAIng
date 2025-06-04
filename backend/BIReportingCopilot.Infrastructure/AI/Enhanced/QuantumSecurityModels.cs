namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Quantum security configuration
/// </summary>
public class QuantumSecurityConfiguration
{
    public bool EnableQuantumResistantSecurity { get; set; } = true;
    public bool EnablePostQuantumCryptography { get; set; } = true;
    public bool EnableQuantumKeyDistribution { get; set; } = false;
    public bool EnableQuantumRandomGeneration { get; set; } = true;
    public QuantumThreatLevel DefaultThreatLevel { get; set; } = QuantumThreatLevel.Medium;
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan EncryptionTimeout { get; set; } = TimeSpan.FromDays(30);
    public TimeSpan SignatureTimeout { get; set; } = TimeSpan.FromDays(7);
    public int KeyRotationIntervalDays { get; set; } = 90;
    public List<string> PreferredAlgorithms { get; set; } = new() { "Kyber-1024", "Dilithium-5" };
}

/// <summary>
/// Quantum security request
/// </summary>
public class QuantumSecurityRequest
{
    public QuantumThreatLevel RequiredThreatLevel { get; set; }
    public List<string> PreferredAlgorithms { get; set; } = new();
    public bool RequireQuantumKeyDistribution { get; set; }
    public bool RequireQuantumRandomness { get; set; }
    public Dictionary<string, object> SecurityRequirements { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Quantum security session
/// </summary>
public class QuantumSecuritySession
{
    public string SessionId { get; set; } = string.Empty;
    public string SecurityId { get; set; } = string.Empty;
    public QuantumThreatLevel ThreatLevel { get; set; }
    public PostQuantumAlgorithmSuite AlgorithmSuite { get; set; } = new();
    public List<QuantumKeyPair> KeyPairs { get; set; } = new();
    public QuantumKeyDistributionSession? QKDSession { get; set; }
    public QuantumSecurityLevel SecurityLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public QuantumSecurityStatus Status { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Post-quantum algorithm suite
/// </summary>
public class PostQuantumAlgorithmSuite
{
    public string SuiteId { get; set; } = string.Empty;
    public List<PostQuantumAlgorithm> Algorithms { get; set; } = new();
    public QuantumThreatLevel ThreatLevel { get; set; }
    public QuantumSecurityLevel SecurityLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Post-quantum algorithm
/// </summary>
public class PostQuantumAlgorithm
{
    public string Name { get; set; } = string.Empty;
    public AlgorithmType Type { get; set; }
    public int SecurityLevel { get; set; }
    public int KeySize { get; set; }
    public string Family { get; set; } = string.Empty;
    public bool IsNISTApproved { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Quantum key pair
/// </summary>
public class QuantumKeyPair
{
    public string KeyId { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();
    public byte[] PrivateKey { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public QuantumSecurityLevel SecurityLevel { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Quantum threat assessment
/// </summary>
public class QuantumThreatAssessment
{
    public string AssessmentId { get; set; } = string.Empty;
    public QuantumThreatLevel ThreatLevel { get; set; }
    public List<QuantumThreat> IdentifiedThreats { get; set; } = new();
    public double ThreatScore { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Quantum threat
/// </summary>
public class QuantumThreat
{
    public string ThreatId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuantumThreatType Type { get; set; }
    public double Severity { get; set; }
    public double Probability { get; set; }
    public List<string> AffectedAlgorithms { get; set; } = new();
    public List<string> Mitigations { get; set; } = new();
}

/// <summary>
/// Quantum encryption result
/// </summary>
public class QuantumEncryptionResult
{
    public string EncryptionId { get; set; } = string.Empty;
    public string SecurityId { get; set; } = string.Empty;
    public PostQuantumAlgorithm Algorithm { get; set; } = new();
    public byte[] EncryptedData { get; set; } = Array.Empty<byte>();
    public Dictionary<string, object> EncryptionMetadata { get; set; } = new();
    public QuantumProof QuantumProof { get; set; } = new();
    public QuantumSecurityLevel SecurityLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Quantum decryption result
/// </summary>
public class QuantumDecryptionResult
{
    public string DecryptionId { get; set; } = string.Empty;
    public string EncryptionId { get; set; } = string.Empty;
    public byte[] DecryptedData { get; set; } = Array.Empty<byte>();
    public QuantumProofVerification VerificationResult { get; set; } = new();
    public QuantumSecurityLevel SecurityLevel { get; set; }
    public DateTime DecryptedAt { get; set; }
    public bool Success { get; set; }
}

/// <summary>
/// Quantum signature result
/// </summary>
public class QuantumSignatureResult
{
    public string SignatureId { get; set; } = string.Empty;
    public string SecurityId { get; set; } = string.Empty;
    public PostQuantumAlgorithm Algorithm { get; set; } = new();
    public byte[] Signature { get; set; } = Array.Empty<byte>();
    public Dictionary<string, object> SignatureMetadata { get; set; } = new();
    public QuantumProof QuantumProof { get; set; } = new();
    public QuantumSecurityLevel SecurityLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Quantum verification result
/// </summary>
public class QuantumVerificationResult
{
    public string VerificationId { get; set; } = string.Empty;
    public string SignatureId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public bool QuantumProofValid { get; set; }
    public bool SignatureValid { get; set; }
    public QuantumSecurityLevel SecurityLevel { get; set; }
    public DateTime VerifiedAt { get; set; }
    public double TrustScore { get; set; }
}

/// <summary>
/// Quantum proof
/// </summary>
public class QuantumProof
{
    public string ProofId { get; set; } = string.Empty;
    public string Algorithm { get; set; } = string.Empty;
    public byte[] ProofData { get; set; } = Array.Empty<byte>();
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Quantum proof verification
/// </summary>
public class QuantumProofVerification
{
    public bool IsValid { get; set; }
    public string ProofId { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
    public double TrustLevel { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}

/// <summary>
/// Quantum key distribution session
/// </summary>
public class QuantumKeyDistributionSession
{
    public string SessionId { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public byte[] QuantumKey { get; set; } = Array.Empty<byte>();
    public double KeyRate { get; set; }
    public double ErrorRate { get; set; }
    public QuantumSecurityLevel SecurityLevel { get; set; }
    public DateTime EstablishedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Encryption context
/// </summary>
public class EncryptionContext
{
    public string DataType { get; set; } = string.Empty;
    public QuantumSecurityLevel RequiredSecurityLevel { get; set; }
    public bool RequireQuantumProof { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Decryption context
/// </summary>
public class DecryptionContext
{
    public bool VerifyQuantumProof { get; set; } = true;
    public QuantumSecurityLevel MinimumSecurityLevel { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Signature context
/// </summary>
public class SignatureContext
{
    public string SignatureType { get; set; } = string.Empty;
    public QuantumSecurityLevel RequiredSecurityLevel { get; set; }
    public bool RequireQuantumProof { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Verification context
/// </summary>
public class VerificationContext
{
    public bool VerifyQuantumProof { get; set; } = true;
    public QuantumSecurityLevel MinimumSecurityLevel { get; set; }
    public double MinimumTrustScore { get; set; } = 0.8;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Security event
/// </summary>
public class SecurityEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public SecurityEventType EventType { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public QuantumThreatLevel ThreatLevel { get; set; }
    public List<string> AlgorithmsUsed { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Quantum security analytics
/// </summary>
public class QuantumSecurityAnalytics
{
    public TimeSpan Period { get; set; }
    public int TotalSecuritySessions { get; set; }
    public int QuantumThreatDetections { get; set; }
    public int EncryptionOperations { get; set; }
    public int SignatureOperations { get; set; }
    public Dictionary<string, int> AlgorithmUsageDistribution { get; set; } = new();
    public Dictionary<QuantumThreatLevel, int> ThreatLevelDistribution { get; set; } = new();
    public Dictionary<string, double> SecurityLevelMetrics { get; set; } = new();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Enums for quantum security
/// </summary>
public enum QuantumThreatLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum QuantumSecurityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Maximum = 4
}

public enum QuantumSecurityStatus
{
    Initializing,
    Active,
    Expired,
    Compromised,
    Revoked
}

public enum AlgorithmType
{
    KeyEncapsulation,
    DigitalSignature,
    SymmetricEncryption,
    HashFunction
}

public enum QuantumThreatType
{
    ShorsAlgorithm,
    GroversAlgorithm,
    QuantumCryptanalysis,
    PostQuantumVulnerability,
    ImplementationWeakness
}

public enum SecurityEventType
{
    QuantumSecurityInitialized,
    QuantumThreatDetected,
    QuantumEncryption,
    QuantumDecryption,
    QuantumSignature,
    QuantumVerification,
    KeyRotation,
    SecurityBreach
}
