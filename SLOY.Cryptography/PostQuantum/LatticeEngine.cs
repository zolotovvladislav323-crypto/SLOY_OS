using System.Security.Cryptography;

namespace SLOY.Cryptography.PostQuantum;

public class LatticeEngine
{
    private readonly int _dimension;
    private readonly int _modulus;

    public LatticeEngine(int dimension = 512, int modulus = 12289)
    {
        _dimension = dimension;
        _modulus = modulus;
    }

    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
    {
        var privateKey = GenerateRandomVector(_dimension);
        var error = GenerateRandomVector(_dimension);
        var a = GenerateRandomMatrix(_dimension);
        var publicKey = new byte[_dimension * sizeof(int)];

        for (int i = 0; i < _dimension; i++)
        {
            long sum = 0;
            for (int j = 0; j < _dimension; j++)
                sum += (long)a[i][j] * privateKey[j];
            sum = ((sum + error[i]) % _modulus + _modulus) % _modulus;
            BitConverter.GetBytes((int)sum).CopyTo(publicKey, i * sizeof(int));
        }

        return (publicKey, privateKey.SelectMany(BitConverter.GetBytes).ToArray());
    }

    public byte[] Encrypt(byte[] message, byte[] publicKey)
    {
        var msg = new int[_dimension];
        for (int i = 0; i < Math.Min(message.Length / sizeof(int), _dimension); i++)
            msg[i] = BitConverter.ToInt32(message, i * sizeof(int));

        var e1 = GenerateRandomVector(_dimension);
        var e2 = GenerateRandomVector(_dimension);
        var ciphertext = new List<byte>();

        for (int i = 0; i < _dimension; i++)
        {
            var val = (msg[i] + e1[i] + e2[i]) % _modulus;
            ciphertext.AddRange(BitConverter.GetBytes(val));
        }

        return ciphertext.ToArray();
    }

    public byte[] Decrypt(byte[] ciphertext, byte[] privateKey)
    {
        var priv = new int[_dimension];
        for (int i = 0; i < _dimension; i++)
            priv[i] = BitConverter.ToInt32(privateKey, i * sizeof(int));

        var decrypted = new List<byte>();
        for (int i = 0; i < _dimension; i++)
        {
            var val = BitConverter.ToInt32(ciphertext, i * sizeof(int));
            var dec = (val - priv[i]) % _modulus;
            if (dec < 0) dec += _modulus;
            decrypted.AddRange(BitConverter.GetBytes(dec));
        }

        return decrypted.ToArray();
    }

    private int[] GenerateRandomVector(int size)
    {
        var vector = new int[size];
        for (int i = 0; i < size; i++)
            vector[i] = RandomNumberGenerator.GetInt32(-3, 4);
        return vector;
    }

    private int[][] GenerateRandomMatrix(int size)
    {
        var matrix = new int[size][];
        for (int i = 0; i < size; i++)
        {
            matrix[i] = new int[size];
            for (int j = 0; j < size; j++)
                matrix[i][j] = RandomNumberGenerator.GetInt32(0, _modulus);
        }
        return matrix;
    }
}