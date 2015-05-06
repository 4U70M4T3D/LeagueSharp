using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Automated_Evelyn
{
    public class AutomatedLybrary
    {
        
        // As I gain more experience/knowledge with programming ill expand this class with more useful functions as well as improve them.
        public static Vector3 BestPosition(Spell spell, float spellRadius)
        {
            float x = 0;
            float y = 0;
            float z = 0;
            var playercount = 0;

            List<Vector3> lstEnemyPosition = new List<Vector3>();
            List<int> lstEnemiesitCount = new List<int>();
            List<Vector3> lstBestPositions = new List<Vector3>();

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(enemy => enemy.IsValidTarget(spell.Range + spellRadius / 2)))
            {
                lstEnemyPosition.Add(enemy.Position);
                x += enemy.Position.X;
                y += enemy.Position.Y;
                z += enemy.Position.Z;
                playercount++;
            }
            Vector3 centroid = new Vector3(x / playercount, y / playercount, z / playercount);

            if (playercount >= 2)
            {
                for (int i = 0; i <= playercount - 1; i++)
                {
                    Vector3 newPosition = new Vector3((lstEnemyPosition[i].X + centroid.X) / 2,
                        (lstEnemyPosition[i].Y + centroid.Y) / 2, (lstEnemyPosition[i].Z + centroid.Z) / 2);
                    lstEnemiesitCount.Add(newPosition.CountEnemiesInRange(spellRadius));
                    lstBestPositions.Add(newPosition);
                }

                for (int j = 0; j < lstEnemiesitCount.Count - 1; j++)
                {
                    int minKey = j;

                    for (int k = j + 1; k < lstEnemiesitCount.Count - 1; k++)
                    {
                        if (lstEnemiesitCount[k] > lstEnemiesitCount[minKey])
                        {
                            minKey = k;
                        }
                    }
                    Vector3 temp = lstBestPositions[minKey];
                    int tmp = lstEnemiesitCount[minKey];
                    lstEnemiesitCount[minKey] = lstEnemiesitCount[j];
                    lstBestPositions[minKey] = lstBestPositions[j];
                    lstEnemiesitCount[j] = tmp;
                    lstBestPositions[j] = temp;
                }

                if (centroid.CountEnemiesInRange(spellRadius) >= lstBestPositions[0].CountEnemiesInRange(spellRadius))
                {
                    return centroid;
                }
                return lstBestPositions[0];
            }
            return TargetSelector.GetTarget(spell.Range, TargetSelector.DamageType.Magical).Position;
        }
    }
}
