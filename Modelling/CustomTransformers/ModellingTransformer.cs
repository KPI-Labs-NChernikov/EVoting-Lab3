using Algorithms;
using Algorithms.Abstractions;
using Algorithms.Common;
using Modelling.Models;

namespace Modelling.CustomTransformers;
public sealed class ModellingTransformer : IObjectToByteArrayTransformer
{
    public bool CanTransform(Type type)
    {
        return type == typeof(Ballot) 
            || type == typeof(SignedData<Ballot>) 
            || type == typeof(SignedData<Guid>) 
            || type == typeof(SignedData<string>) 
            || type == typeof(Guid)
            || type == typeof(string);
    }

    private readonly GuidTransformer _guidTransformer = new ();

    public T? ReverseTransform<T>(byte[] data)
    {
        var span = data.AsSpan();
        if (typeof(T) == typeof(Ballot))
        {
            var currentStart = 0;
            var voterId = new Guid(span[..PublicConstants.GuidSize]);
            currentStart += PublicConstants.GuidSize;
            var registrationId = new Guid(span.Slice(currentStart, PublicConstants.GuidSize));
            currentStart += PublicConstants.GuidSize;
            var candidateId = BitConverter.ToInt32(span.Slice(currentStart, PublicConstants.IntSize));

            return (T)(object)new Ballot(voterId, registrationId, candidateId);
        }
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(SignedData<>))
        {
            var actualData = GetType().GetMethod(nameof(ReverseTransform))!.MakeGenericMethod(typeof(T).GenericTypeArguments[0])
                .Invoke(this, new object[] { span.Slice(0, span.Length - PublicConstants.DSASignatureSize).ToArray() });
            var signature = span.Slice(span.Length - PublicConstants.DSASignatureSize, PublicConstants.DSASignatureSize);
            return (T)Activator.CreateInstance(typeof(T), actualData!, signature.ToArray())!;
        }
        if (typeof(T) == typeof(Guid))
        {
            return _guidTransformer.ReverseTransform<T>(data);
        }
        if(typeof(T) == typeof(string))
        {
            return (T)(object)PublicConstants.Encoding.GetString(data);
        }

        throw new NotSupportedException($"The type {typeof(T)} is not supported.");
    }

    public byte[] Transform(object obj)
    {
        if (obj.GetType() == typeof(Ballot))
        {
            var ballot = (Ballot)obj;
            using var stream = new MemoryStream();
            stream.Write(ballot.VoterId.ToByteArray());
            stream.Write(ballot.RegistrationId.ToByteArray());
            stream.Write(BitConverter.GetBytes(ballot.CandidateId));
            return stream.ToArray();
        }
        if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(SignedData<>))
        {
            using var stream = new MemoryStream();
            stream.Write(Transform(obj.GetType().GetProperty("Data")!.GetValue(obj)!));
            stream.Write((byte[])obj.GetType().GetProperty("Signature")!.GetValue(obj)!);
            return stream.ToArray();
        }
        if (obj.GetType() == typeof(Guid))
        {
            return _guidTransformer.Transform(obj);
        }
        if (obj.GetType() == typeof (string))
        {
            return PublicConstants.Encoding.GetBytes((string)obj);
        }

        throw new NotSupportedException($"The type {obj.GetType()} is not supported.");
    }
}
