using Algorithms.Common;
using Algorithms.DSA;
using Algorithms.ElGamal;
using Demo;
using Modelling.CustomTransformers;
using Modelling.Models;

var randomProvider = new RandomProvider();

var objectToByteArrayTransformer = new ObjectToByteArrayTransformer();
objectToByteArrayTransformer.TypeTransformers.Add(new GuidTransformer());
objectToByteArrayTransformer.TypeTransformers.Add(new ModellingTransformer());

var encryptionProvider = new ElGamalEncryptionProvider();
var encryptionKeyGenerator = new ElGamalKeysGenerator();

var signatureProvider = new DSASignatureProvider();
var signatureKeyGenerator = new DSAKeysGenerator();

var dataFactory = new DemoDataFactory(encryptionProvider, encryptionKeyGenerator, signatureProvider, signatureKeyGenerator, objectToByteArrayTransformer);
var candidates = dataFactory.CreateCandidates();
var voters = dataFactory.CreateVoters();
var registrationBureau = dataFactory.CreateRegistrationBureau(voters);

var printer = new ModellingPrinter(encryptionProvider, encryptionKeyGenerator, signatureProvider, signatureKeyGenerator, objectToByteArrayTransformer);

printer.PrintUsualRegistration(registrationBureau, voters);
printer.PrintDoubleRegistration(registrationBureau, randomProvider.NextItem(voters));

var registeredVoters = registrationBureau.FinishRegistration();
var electionCommission = new ElectionCommission(candidates, registeredVoters, signatureKeyGenerator.Generate(), encryptionKeyGenerator.Generate(), signatureProvider, encryptionProvider, objectToByteArrayTransformer);

printer.PrintUsualVoting(electionCommission, dataFactory.CreateVotersWithCandidateIds(voters.SkipLast(1).ToList()));
printer.PrintVotingWithIncorrectBallot(electionCommission, voters[^1]);
printer.PrintVotingWithDoubleBallot(electionCommission, 2, voters[^1]);

printer.PrintVotingResults(electionCommission);

printer.PrintVotingAfterCompletion(electionCommission, randomProvider.NextItem(candidates).Id, randomProvider.NextItem(voters));
