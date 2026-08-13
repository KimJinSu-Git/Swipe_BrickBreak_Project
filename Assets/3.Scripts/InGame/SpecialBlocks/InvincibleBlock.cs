using UnityEngine;

namespace Bird.InGame.SpecialBlocks
{
    public class InvincibleBlock : Block
    {
        public override bool CausesGameOver => false;

        public override int TakeDamage(int damage)
        {
            return 0; 
        }
        
        public override void Heal(int amount)
        {
            
        }
    }
}