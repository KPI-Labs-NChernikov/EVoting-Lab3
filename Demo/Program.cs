using Algorithms.Common;
using Modelling.CustomTransformers;
using Modelling.Models;

var objectToByteArrayTransformer = new ObjectToByteArrayTransformer();
objectToByteArrayTransformer.TypeTransformers.Add(new GuidTransformer());
objectToByteArrayTransformer.TypeTransformers.Add(new ModellingTransformer());

var ballot = new Ballot(Guid.NewGuid(), Guid.NewGuid(), 3);
var signedBallot = new SignedData<Ballot>(ballot,new byte[64]);
var byteArray = objectToByteArrayTransformer.Transform(signedBallot);
var signedBallotCopy = objectToByteArrayTransformer.ReverseTransform<SignedData<Ballot>>(byteArray);

var guid = Guid.NewGuid();
var signedGuid = new SignedData<Guid>(guid, new byte[64]);
var byteArray2 = objectToByteArrayTransformer.Transform(signedGuid);
var signedGuidCopy = objectToByteArrayTransformer.ReverseTransform<SignedData<Guid>>(byteArray2);

Console.WriteLine();
