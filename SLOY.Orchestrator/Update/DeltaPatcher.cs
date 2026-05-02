namespace SLOY.Orchestrator.Update;

public class DeltaPatcher
{
    public byte[] CreateDelta(byte[] oldData, byte[] newData)
    {
        var delta = new List<byte>();
        var i = 0;

        while (i < newData.Length)
        {
            if (i < oldData.Length && oldData[i] == newData[i])
            {
                var start = i;
                while (i < oldData.Length && i < newData.Length && oldData[i] == newData[i])
                    i++;
                WriteCopyCommand(delta, start, i - start);
            }
            else
            {
                var start = i;
                var diffData = new List<byte>();

                while (i < newData.Length && (i >= oldData.Length || oldData[i] != newData[i]))
                {
                    diffData.Add(newData[i]);
                    i++;
                }

                WriteInsertCommand(delta, diffData.ToArray());
            }
        }

        return delta.ToArray();
    }

    public byte[] Apply(byte[] oldData, byte[] delta)
    {
        var result = new List<byte>();
        var pos = 0;

        while (pos < delta.Length)
        {
            var command = delta[pos++];

            if (command == 0x01)
            {
                var offset = BitConverter.ToInt32(delta, pos); pos += 4;
                var length = BitConverter.ToInt32(delta, pos); pos += 4;
                result.AddRange(oldData.Skip(offset).Take(length));
            }
            else if (command == 0x02)
            {
                var length = BitConverter.ToInt32(delta, pos); pos += 4;
                result.AddRange(delta.Skip(pos).Take(length));
                pos += length;
            }
        }

        return result.ToArray();
    }

    private void WriteCopyCommand(List<byte> delta, int offset, int length)
    {
        delta.Add(0x01);
        delta.AddRange(BitConverter.GetBytes(offset));
        delta.AddRange(BitConverter.GetBytes(length));
    }

    private void WriteInsertCommand(List<byte> delta, byte[] data)
    {
        delta.Add(0x02);
        delta.AddRange(BitConverter.GetBytes(data.Length));
        delta.AddRange(data);
    }
}