using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AuctionSequence : IEnumerable<BiddingRound>
{
    [SerializeField]
    private AuctionStage[] stages;
    private static IEnumerable<BiddingRound> BiddingRounds(AuctionStage[] stages)
    {
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].Promote(out BiddingRound round))
            {
                Debug.Log($"[{string.Join(", ", round.players.Select(p => p?.gameObject.name ?? "None" ))}]");
                yield return round;
            }
        }
    }

    public IEnumerator<BiddingRound> GetEnumerator()
    {
        return BiddingRounds(stages).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return BiddingRounds(stages).GetEnumerator();
    }
}
