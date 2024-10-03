using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace NMocker
{
    //public class Verifier
    //{
    //    public static VerificationGroup Times(int times)
    //    {
    //        return new VerificationGroup(times == 0 ? "no call" : string.Format("to call {0} times", times), times.Equals);
    //    }

    //    public static VerificationGroup AtLeast(int times)
    //    {
    //        return new VerificationGroup(string.Format("to call at least {0} times", times), i => i >= times);
    //    }

    //    public static VerificationGroup AtMost(int times)
    //    {
    //        return new VerificationGroup(string.Format("to call at most {0} times", times), i => i <= times);
    //    }
    //    public static VerificationGroup Call(Expression<Action> invocation)
    //    {
    //        return AtLeast(1).Call(invocation, 1);
    //    }

    //    public static VerificationGroup Never { get { return Times(0); } }

    //    public static VerificationGroup Once { get { return Times(1); } }
    //}

    //public class Verification
    //{
    //    public readonly InvocationMatcher invocationMatcher;
    //    private readonly string position;
    //    private readonly List<Invocation> hits = new List<Invocation>();

    //    public Verification(InvocationMatcher invocationMatcher, string position)
    //    {
    //        this.invocationMatcher = invocationMatcher;
    //        this.position = position;
    //    }

    //    public string MessageHitFrom(int hit)
    //    {
    //        return string.Format("hit({0}) from {1} => ", hit, position);
    //    }

    //    public string UnsatisfiedMessage()
    //    {
    //        return string.Format("Unsatisfied invocation at {0}", position);
    //    }
    //}

    //public class VerificationGroup
    //{
    //    private Predicate<int> testTimes;
    //    private string expectationMessage;
    //    private List<Verification> verifications = new List<Verification>();

    //    public VerificationGroup(string message, Predicate<int> testTimes)
    //    {
    //        this.expectationMessage = message;
    //        this.testTimes = testTimes;
    //    }

    //    public VerificationGroup Call(Expression<Action> invocation, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
    //    {
    //        this.verifications.Add(new Verification(new InvocationMatcher(invocation), string.Format("{0}:{1}",file, line)));
    //        return this;
    //    }

    //    public void Verify()
    //    {
    //        List<Invocation> hits = new List<Invocation>();
    //        Dictionary<Invocation, Verification> invocationHits = new Dictionary<Invocation, Verification>();

    //        for(int i=0; i<Invocation.invocations.Count;)
    //        {
    //            Invocation invocation = Invocation.invocations[i];
    //            Verification verification = verifications[0];

    //            if (verification.invocationMatcher.Matches(invocation))
    //            {
    //                hits.Add(invocation);
    //                invocationHits.Add(invocation, verification);
    //            }
    //            i++;
    //        }

    //        if (!testTimes.Invoke(hits.Count))
    //        {
    //            StringBuilder message = new StringBuilder();
    //            message.Append(verifications[0].UnsatisfiedMessage());
    //            message.Append("\nAll invocations:\n");

    //            int width = invocationHits.Keys.Select(invocation => invocationHits[invocation].MessageHitFrom(hits.IndexOf(invocation) + 1).Length)
    //                .DefaultIfEmpty(0).Max();

    //            foreach(Invocation invocation in Invocation.invocations)
    //            {
    //                message.Append("    ");
    //                if (invocationHits.ContainsKey(invocation))
    //                {
    //                    message.Append(invocationHits[invocation].MessageHitFrom(hits.IndexOf(invocation) + 1));
    //                    message.Append(invocation.Dump()).Append('\n');
    //                }
    //                else
    //                {
    //                    message.Append(new String(' ', width));
    //                    message.Append(invocation.Dump()).Append('\n');
    //                }
    //            }

    //            message.Append(string.Format("Expected {0}, but actually call {1} times.", expectationMessage, hits.Count));
    //            throw new UnexpectedCallException(message.ToString());
    //        }

    //        //int groupHitNumber = 0;
    //        //List<HitResult> hitResults = new List<HitResult>();
    //        //for (int i = 0; i < Invocation.invocations.Count;)
    //        //{
    //        //    int hit = 0;
    //        //    int j = 0;
    //        //    while (j < verifications.Count && i < Invocation.invocations.Count)
    //        //    {
    //        //        Invocation invocation = Invocation.invocations[i++];
    //        //        Verification verification = verifications[j];

    //        //        if (verification.Hit(invocation))
    //        //        {
    //        //            hitResults.Add(new Hit(groupHitNumber + 1, invocation, verification));
    //        //            hit++;
    //        //            j++;
    //        //        }
    //        //        else if (j + 1 < verifications.Count && verifications[j + 1].Hit(invocation))
    //        //        {
    //        //            hitResults.Add(new Hit(groupHitNumber + 1, invocation, verifications[j + 1]));
    //        //            hit++;
    //        //            j += 2;
    //        //        }
    //        //        else
    //        //            hitResults.Add(new HitResult(invocation));
    //        //    }
    //        //    if (hit == verifications.Count)
    //        //        groupHitNumber++;
    //        //}



    //        //Verification verification = verifications[0]; //1

    //        //List<HitResult> hitResults = new List<HitResult>();

    //        //int hitCount = 0;
    //        //foreach (Invocation invocation in Invocation.invocations)
    //        //{
    //        //    if(verification.Hit(invocation))
    //        //    {

    //        //    }
    //        //    hitResults.Add(verification.HitBk(invocation));
    //        //}

    //        //if (!testTimes.Invoke(verification.hitCount))
    //        //{
    //        //    StringBuilder message = new StringBuilder();
    //        //    message.Append(verification.UnsatisfiedMessage());
    //        //    message.Append("\nAll invocations:\n");
    //        //    int maxWidth = hitResults.Select(h => h.HitMessage(0).Length).DefaultIfEmpty(0).Max();
    //        //    foreach (HitResult hit in hitResults)
    //        //        message.Append("    ").Append(hit.HitMessage(maxWidth)).Append(hit.CalledMessage).Append('\n');
    //        //    message.Append(string.Format("Expected {0}, but actually call {1} times.", expectationMessage, verification.hitCount));
    //        //    throw new UnexpectedCallException(message.ToString());
    //        //}
    //    }
    //}
}
