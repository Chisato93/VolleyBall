using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class MatchHistoryRecord
{
    public string SavedAtUtc;
    public bool IsWin;
    public int PlayerScore;
    public int OpponentScore;
    public int MatchEvaluationScore;
    public int PaidAllowance;
    public List<PlayerMatchStats> Players;
}

public static class MatchHistoryStore
{
    [Serializable]
    private class HistoryData
    {
        public int Version;
        public List<MatchHistoryRecord> Matches;
    }

    public static string FilePath => Path.Combine(Application.persistentDataPath, "match-history.json");

    public static bool TryLoad(out List<MatchHistoryRecord> matches)
    {
        matches = new List<MatchHistoryRecord>();

        try
        {
            if (!File.Exists(FilePath))
            {
                return true;
            }

            HistoryData data = JsonUtility.FromJson<HistoryData>(File.ReadAllText(FilePath));
            if (data == null || data.Version != 1 || data.Matches == null)
            {
                throw new InvalidDataException("경기 기록 파일 형식을 확인할 수 없습니다.");
            }

            foreach (MatchHistoryRecord match in data.Matches)
            {
                if (match == null || match.Players == null || string.IsNullOrEmpty(match.SavedAtUtc))
                {
                    throw new InvalidDataException("경기 기록에 필요한 정보가 없습니다.");
                }
            }

            matches = data.Matches;
            return true;
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            Debug.LogWarning($"경기 기록을 읽지 못했습니다. 기존 파일을 유지합니다. {exception.Message}");
            return false;
        }
    }

    public static bool SaveMatch(MatchManager matchManager, int evaluationScore, int paidAllowance)
    {
        if (matchManager == null || !TryLoad(out List<MatchHistoryRecord> matches))
        {
            return false;
        }

        // 다음 경기에서 원본 기록이 초기화되어도 저장할 기록은 유지합니다.
        MatchStats snapshot = new MatchStats();
        snapshot.CopyFrom(matchManager.CurrentMatchStats);
        matches.Add(new MatchHistoryRecord
        {
            SavedAtUtc = DateTime.UtcNow.ToString("O"),
            IsWin = matchManager.PlayerScore > matchManager.OpponentScore,
            PlayerScore = matchManager.PlayerScore,
            OpponentScore = matchManager.OpponentScore,
            MatchEvaluationScore = evaluationScore,
            PaidAllowance = paidAllowance,
            Players = new List<PlayerMatchStats>(snapshot.PlayerStats)
        });

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string temporaryPath = FilePath + ".tmp";
            string json = JsonUtility.ToJson(new HistoryData { Version = 1, Matches = matches }, true);
            File.WriteAllText(temporaryPath, json);

            // 파일 작성을 마친 뒤 교체해 기존 기록이 중간에 잘리지 않게 합니다.
            if (File.Exists(FilePath))
            {
                File.Replace(temporaryPath, FilePath, null);
            }
            else
            {
                File.Move(temporaryPath, FilePath);
            }

            return true;
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            Debug.LogWarning($"경기 기록을 저장하지 못했습니다. {exception.Message}");
            return false;
        }
    }

    public static bool Clear()
    {
        try
        {
            File.Delete(FilePath);
            File.Delete(FilePath + ".tmp");
            return true;
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            Debug.LogWarning($"경기 기록을 초기화하지 못했습니다. {exception.Message}");
            return false;
        }
    }

    private static bool IsStorageException(Exception exception)
    {
        return exception is IOException || exception is UnauthorizedAccessException
            || exception is ArgumentException || exception is NotSupportedException
            || exception is System.Security.SecurityException;
    }
}
