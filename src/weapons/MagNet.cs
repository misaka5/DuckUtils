using DuckGame;
using System;

namespace DuckGame.DuckUtils {

    [EditorGroup("duckutils")]
    [BaggedProperty("isFatal", false)]
    public class MagNet : Gun
    {
		public static readonly ATProvider ExplosionShrapnel = ExplosionAT.From<ATShrapnel>().WithRange(4f, 10f);

		public static readonly float MaxRange = 100f;
		public static readonly float RangeAllocationSpeed = 60f;
		public static readonly float ExplosionRangeSquared = 15f * 15f;
		public static readonly float CapturingSpeed = 20.6667f;
		public static readonly float SpeedApproachMod = 0.05f;
		public static readonly float ExplosionTime = 1f;

        public readonly SpriteMap spriteMap;

		public StateBinding StateBinding { get; private set; }

        private byte stateIndex {
            get {
                return (byte)CurrentState;
            }

            set {
                CurrentState = (State)value;
            }
        }

		private State state = State.Idle;
        private State CurrentState {
			get {
				return state;
			}

			set {
				if(state != value) {
					//UpdateState(value);
					state = value;
				}
			}
		}
		

		//state vars

		private float range;
		private Gun target;
		private float captureTime;

		private ImpactedFrom attachedSide;
		private MaterialThing attachedTo;
		private Vec2 attachedOwnerPos;

		private float AttachedAngle {
			get {
				switch(attachedSide) {
					case ImpactedFrom.Top: return -MathHelper.Pi;
					case ImpactedFrom.Bottom: return 0f;
					case ImpactedFrom.Left: return MathHelper.Pi / 2;
					case ImpactedFrom.Right: return -MathHelper.Pi / 2;
					default: return 0f;
				}
			}
		}
        
        public MagNet(float xval, float yval)
            : base(xval, yval)
        {
			StateBinding = new StateBinding("stateIndex");
			
			ammo = 5;
			_ammoType = new ATShrapnel();
			
			_type = "gun";
			_editorName = "MagNet";
            _bio = "h";

			graphic = spriteMap = new SpriteMap(DuckUtils.GetAsset("weapons/magnet.png"), 16, 16);
			
			spriteMap.AddAnimation("active", 0.1f, true, 1, 2);
			spriteMap.AddAnimation("idle", 0.1f, true, 0);
			spriteMap.SetAnimation("idle");

			center = new Vec2(8f, 4f);
			collisionOffset = new Vec2(-8f, 0f);
			collisionSize = new Vec2(16f, 10f);
			_barrelOffsetTL = new Vec2(20f, 8f);
			_fullAuto = true;
			_fireWait = 1.75f;
			_kickForce = 0.6f;
			_holdOffset = new Vec2(0f, -5f);
            _fireSound = "";
        }

		public override void OnPressAction()
		{
			if(CurrentState == State.Idle) {
				CurrentState = State.Attaching;
				SFX.Play("pullPin");
				spriteMap.SetAnimation("active");
				canPickUp = false;
				if(duck != null) duck.doThrow = true;
			}
		}

		public override void OnHoldAction() {}
		public override void OnReleaseAction() {}

		public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
		{
			if(CurrentState == State.Attaching) {
				attachedTo = with;
				attachedSide = from;
				attachedOwnerPos = with.position;

				CurrentState = State.Active;
			}
		}

		private bool CheckAccessibility(PhysicsObject obj, bool targeted) {
			if(obj.removeFromLevel || !obj.active) return false;
			if(!targeted && Math.Abs(obj.hSpeed) + Math.Abs(obj.vSpeed) < 2f) return false;
			if(targeted && (position - obj.position).lengthSq > 3 * MaxRange * MaxRange) return false;
			
			Vec2 hit;
			MaterialThing t = Level.CheckRay<MaterialThing>(position, obj.position, this, out hit);
			return t == null || t.thickness <= 1f || t == obj;
		}

		private void ApproachTarget() {
			Vec2 delta = position - target.position;
			Vec2 grav = delta.normalized * (CapturingSpeed * (1f - delta.lengthSq / (MaxRange * MaxRange)));
								
			target.hSpeed += (grav.x - target.hSpeed) * SpeedApproachMod;
			target.vSpeed += (grav.y - target.vSpeed) * SpeedApproachMod;
		}

		public override void Update() 
        {
			base.Update();

			if(isServerForObject) {
				if(attachedTo != null) {
					if(attachedTo.removeFromLevel) {
						attachedTo = null;
						gravMultiplier = 1f;
						CurrentState = State.Attaching;
					} else {
						hSpeed = vSpeed = 0f;
						gravMultiplier = 0f;
						angle = AttachedAngle;
					}
				}

				switch(CurrentState) {
					case State.Active: 
						if(range < MaxRange) range += Maths.IncFrameTimer() * RangeAllocationSpeed;
						foreach(Gun obj in Level.CheckCircleAll<Gun>(position, range)) {
							if(obj != this && CheckAccessibility(obj, false)) {
								target = obj;
								Fondle(obj);

								CurrentState = State.Targeted;
								break;
							}
						}
						break;

					case State.Targeted:
						if(target != null && CheckAccessibility(target, true)) {
							Vec2 delta = position - target.position;

							if(delta.lengthSq < ExplosionRangeSquared) {
								CurrentState = State.Exploding;
							} else {
								ApproachTarget();
							}
						} else CurrentState = State.Active;
						break;

					case State.Exploding:
						if(captureTime < ExplosionTime && target != null && CheckAccessibility(target, true)) {
							ApproachTarget();
							
							captureTime += Maths.IncFrameTimer();
							if(Rando.Float(1f) < 2 / 60f) target.PressAction();
						} else {
							captureTime = 0f;

							Explosion.Create(this, ExplosionShrapnel);

							if(target != null) Level.Remove(target);
							CurrentState = State.Active;

							if(--ammo == 0) Level.Remove(this);
						}
						break;
				}
			}
		}
		
		public enum State : byte {
            Idle,
            Attaching,
			Active,
			Targeted,
			Exploding
        }
    }
}