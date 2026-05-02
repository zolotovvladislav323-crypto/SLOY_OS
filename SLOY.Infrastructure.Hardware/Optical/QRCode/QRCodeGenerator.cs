namespace SLOY.Infrastructure.Hardware.Optical.QRCode;

public class QRCodeGenerator
{
    private readonly int _moduleSize;
    private readonly int _borderModules;

    public QRCodeGenerator(int moduleSize = 4, int borderModules = 2)
    {
        _moduleSize = moduleSize;
        _borderModules = borderModules;
    }

    public byte[] GenerateBitmap(string data, int version = 3)
    {
        var qrSize = 17 + version * 4;
        var bitmapSize = (qrSize + 2 * _borderModules) * _moduleSize;
        var bitmap = new byte[bitmapSize * bitmapSize];

        var qrData = GenerateQrMatrix(data, version);

        for (int y = 0; y < qrSize; y++)
        {
            for (int x = 0; x < qrSize; x++)
            {
                var moduleValue = qrData[y * qrSize + x] ? (byte)0 : (byte)255;
                FillModule(bitmap, bitmapSize, x + _borderModules, y + _borderModules, moduleValue);
            }
        }

        return bitmap;
    }

    private void FillModule(byte[] bitmap, int bitmapSize, int moduleX, int moduleY, byte value)
    {
        var startX = moduleX * _moduleSize;
        var startY = moduleY * _moduleSize;

        for (int dy = 0; dy < _moduleSize; dy++)
        {
            for (int dx = 0; dx < _moduleSize; dx++)
            {
                var idx = (startY + dy) * bitmapSize + (startX + dx);
                if (idx < bitmap.Length) bitmap[idx] = value;
            }
        }
    }

    private bool[] GenerateQrMatrix(string data, int version)
    {
        var size = 17 + version * 4;
        var matrix = new bool[size * size];

        AddFinderPatterns(matrix, size);
        AddTimingPatterns(matrix, size);
        AddAlignmentPatterns(matrix, size, version);

        var dataBits = EncodeData(data, version);
        PlaceData(matrix, size, dataBits);

        return matrix;
    }

    private void AddFinderPatterns(bool[] matrix, int size)
    {
        for (int p = 0; p < 3; p++)
        {
            int startX = p switch { 0 => 0, 1 => size - 7, _ => 0 };
            int startY = p switch { 0 => 0, 1 => 0, _ => size - 7 };

            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    var isBorder = x == 0 || x == 6 || y == 0 || y == 6;
                    var isInner = x >= 2 && x <= 4 && y >= 2 && y <= 4;
                    matrix[(startY + y) * size + (startX + x)] = isBorder || isInner;
                }
            }
        }
    }

    private void AddTimingPatterns(bool[] matrix, int size)
    {
        for (int i = 8; i < size - 8; i++)
        {
            matrix[6 * size + i] = i % 2 == 0;
            matrix[i * size + 6] = i % 2 == 0;
        }
    }

    private void AddAlignmentPatterns(bool[] matrix, int size, int version)
    {
        if (version < 2) return;

        var positions = GetAlignmentPositions(version);
        foreach (var cy in positions)
        {
            foreach (var cx in positions)
            {
                if ((cx < 9 && cy < 9) || (cx > size - 10 && cy < 9) || (cx < 9 && cy > size - 10))
                    continue;

                for (int y = -2; y <= 2; y++)
                {
                    for (int x = -2; x <= 2; x++)
                    {
                        var isBorder = Math.Abs(x) == 2 || Math.Abs(y) == 2;
                        var isCenter = x == 0 && y == 0;
                        matrix[(cy + y) * size + (cx + x)] = isBorder || isCenter;
                    }
                }
            }
        }
    }

    private int[] GetAlignmentPositions(int version) => new[] { 6, 22, 38 };

    private bool[] EncodeData(string text, int version)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        var bits = new List<bool>();

        bits.AddRange(new[] { false, true, false, false }); // Byte mode

        var lengthBits = version < 10 ? 8 : 16;
        for (int i = lengthBits - 1; i >= 0; i--)
            bits.Add(((bytes.Length >> i) & 1) == 1);

        foreach (var b in bytes)
        {
            for (int i = 7; i >= 0; i--)
                bits.Add(((b >> i) & 1) == 1);
        }

        bits.AddRange(new[] { false, false, false, false }); // Terminator

        while (bits.Count % 8 != 0)
            bits.Add(false);

        var padBytes = new[] { 0xEC, 0x11 };
        int padIndex = 0;
        while (bits.Count < 8 * bytes.Length + 100)
        {
            for (int i = 7; i >= 0; i--)
                bits.Add(((padBytes[padIndex] >> i) & 1) == 1);
            padIndex = (padIndex + 1) % 2;
        }

        return bits.ToArray();
    }

    private void PlaceData(bool[] matrix, int size, bool[] data)
    {
        int dataIndex = 0;
        bool upward = true;

        for (int col = size - 1; col >= 0; col -= 2)
        {
            if (col == 6) col = 5;

            for (int row = 0; row < size; row++)
            {
                int y = upward ? size - 1 - row : row;

                for (int dc = 0; dc < 2; dc++)
                {
                    int x = col - dc;
                    if (x < 0 || matrix[y * size + x]) continue;

                    if (dataIndex < data.Length)
                    {
                        matrix[y * size + x] = data[dataIndex++];
                    }
                }
            }

            upward = !upward;
        }
    }
}