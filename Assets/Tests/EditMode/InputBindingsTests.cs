using MementoMori.Core;
using NUnit.Framework;
using UnityEngine;

namespace MementoMori.Tests.EditMode
{
    public sealed class InputBindingsTests
    {
        [Test]
        public void LastPressedDirectionWinsAndReleaseReturnsToHeldDirection()
        {
            var input = new CardinalMovement();
            Assert.That(input.Resolve(true, false, false, false), Is.EqualTo(Vector2.left));
            Assert.That(input.Resolve(true, false, true, false), Is.EqualTo(Vector2.up));
            Assert.That(input.Resolve(true, false, false, false), Is.EqualTo(Vector2.left));
            Assert.That(input.Resolve(false, false, false, false), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void EveryKeyboardCombinationAndAnalogInputUsesAtMostOneAxis()
        {
            var input = new CardinalMovement();
            for (var mask = 0; mask < 16; mask++)
            {
                var value = input.Resolve((mask & 1) != 0, (mask & 2) != 0, (mask & 4) != 0, (mask & 8) != 0);
                Assert.That(value.x * value.y, Is.Zero);
                Assert.That(value.sqrMagnitude, Is.EqualTo(mask == 0 ? 0f : 1f));
            }
            Assert.That(CardinalMovement.Snap(new Vector2(.8f, .6f)), Is.EqualTo(Vector2.right));
            Assert.That(CardinalMovement.Snap(Vector2.zero), Is.EqualTo(Vector2.zero));
        }
        [Test]
        public void BindingRejectsKeyAlreadyAssignedToAnotherAction()
        {
            var left = InputBindings.Get(GameAction.MoveLeft);
            Assert.That(InputBindings.CanBind(GameAction.MoveRight, left), Is.False);
        }

        [Test]
        public void BindingReservesDialogueAndFallbackMovementKeys()
        {
            Assert.That(InputBindings.CanBind(GameAction.Interact, KeyCode.Escape), Is.False);
            Assert.That(InputBindings.CanBind(GameAction.Pause, KeyCode.Return), Is.False);
            Assert.That(InputBindings.CanBind(GameAction.Grimoire, KeyCode.LeftArrow), Is.False);
        }
    }
}
