namespace CorrentesDaNoite.Chase.States
{
    public abstract class JungleChaseState
    {
        protected JungleChaseSequenceController controller;

        public JungleChaseState(JungleChaseSequenceController controller)
        {
            this.controller = controller;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }
}
