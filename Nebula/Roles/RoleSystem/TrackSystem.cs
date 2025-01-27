namespace Nebula.Roles.RoleSystem;

public static class TrackSystem
{
    public static CustomButton DeadBodySearch_ButtonInitialize(HudManager __instance, Dictionary<byte, Arrow> arrows, Sprite buttonSprite, float duration, float coolDown)
    {
        CustomButton result = null;
        result = new CustomButton(
            () =>
            {

            },
            () => { return !PlayerControl.LocalPlayer.Data.IsDead; },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                foreach (var arrow in arrows.Values) UnityEngine.Object.Destroy(arrow.arrow);
                arrows.Clear();
                result.isEffectActive = false;
                result.Timer = result.MaxTimer;
            },
            buttonSprite,
            Expansion.GridArrangeExpansion.GridArrangeParameter.None,
            __instance,
            Module.NebulaInputManager.abilityInput.keyCode,
            true,
            duration,
            () =>
            {
                foreach (var arrow in arrows.Values) UnityEngine.Object.Destroy(arrow.arrow);
                arrows.Clear();
            }
        );
        result.MaxTimer = result.Timer = coolDown;
        return result;
    }

    public static void DeadBodySearch_MyControlUpdate(bool showFlag, Dictionary<byte, Arrow> arrows, SpriteLoader arrowSprite)
    {
        if (!showFlag)
        {
            if (arrows.Count > 0)
            {
                foreach (var arrow in arrows.Values)
                {
                    UnityEngine.Object.Destroy(arrow.arrow);
                }
                arrows.Clear();
            }
        }
        else
        {
            HashSet<byte> removeKeys = new HashSet<byte>();
            DeadBody[] deadBodies = Helpers.AllDeadBodies();

            //情報の更新および、存在しない死体への矢印を消去
            bool existFlag = false;
            foreach (var entry in arrows)
            {
                existFlag = false;
                foreach (var body in deadBodies)
                {
                    if (body.ParentId == entry.Key)
                    {
                        existFlag = true; break;
                    }
                }
                if (!existFlag)
                {
                    removeKeys.Add(entry.Key);
                    UnityEngine.Object.Destroy(entry.Value.arrow);
                }
            }
            foreach (var id in removeKeys) arrows.Remove(id);


            //あらたな死体を追加
            foreach (var body in deadBodies)
            {
                if (!arrows.ContainsKey(body.ParentId))
                {
                    arrows.Add(body.ParentId, new Arrow(Color.blue, true, arrowSprite.GetSprite()));
                }
            }

            foreach (var body in deadBodies)
            {
                arrows[body.ParentId].Update(body.transform.position);
            }
        }
    }

    public static void PlayerTrack_MyControlUpdate(ref Arrow? arrow, PlayerControl? target, Color color, SpriteLoader arrowSprite)
    {
        if (target == null || target.Data.IsDead)
        {
            if (arrow != null)
            {
                GameObject.Destroy(arrow.arrow);
                arrow = null;
            }
            return;
        }

        if (arrow == null) arrow = new Arrow(color, true, arrowSprite.GetSprite());

        arrow.Update(target.transform.position);
    }

    public static void PlayerTrack_MyControlUpdate(ref Arrow? arrow, Game.PlayerObject? target, Color color, SpriteLoader arrowSprite)
    {
        if (target == null || target.control == null)
        {
            if (arrow != null)
            {
                GameObject.Destroy(arrow.arrow);
                arrow = null;
            }
            return;
        }

        if (target.control.Data.IsDead)
        {
            if (!target.deadBody)
            {
                foreach (var d in Helpers.AllDeadBodies())
                {
                    if (d.ParentId == target.control.PlayerId)
                    {
                        target.deadBody = d;
                        break;
                    }
                }
            }

            if (!target.deadBody)
            {
                if (arrow != null)
                {
                    GameObject.Destroy(arrow.arrow);
                    arrow = null;
                }
                return;
            }

            if (arrow == null) arrow = new Arrow(color, true, arrowSprite.GetSprite());

            arrow.Update(target.deadBody.transform.position);
        }
        else
        {
            if (arrow == null) arrow = new Arrow(color, true, arrowSprite.GetSprite());

            arrow.Update(target.control.transform.position);
        }
    }
}
