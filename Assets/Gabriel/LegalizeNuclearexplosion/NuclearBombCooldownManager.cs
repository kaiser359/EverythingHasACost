using System.Collections;
using UnityEngine;

public static class NuclearBombCooldownManager
{
    // Simple helper to reactivate the bomb object after cooldown seconds
    public static void BeginCooldown(GameObject bomb, float cooldown)
    {
        if (bomb == null) return;
        var runner = bomb.GetComponent<CooldownRunner>();
        if (runner == null) runner = bomb.AddComponent<CooldownRunner>();
        runner.StartCooldown(cooldown);
    }

    private class CooldownRunner : MonoBehaviour
    {
        public void StartCooldown(float cooldown)
        {
            StartCoroutine(Run(cooldown));
        }

        private IEnumerator Run(float cooldown)
        {
            if (cooldown > 0f)
                yield return new WaitForSeconds(cooldown);
            // Reactivate parent bomb
            var bomb = this.gameObject;
            bomb.SetActive(true);
            Destroy(this);
        }
    }
}
