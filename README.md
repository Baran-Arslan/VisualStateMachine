# Unity HFSM with Graph View 🚀

#Video 
[![Unity HFSM with Graph View](https://img.youtube.com/vi/mUIsRAAXgM0/0.jpg)](https://www.youtube.com/watch?v=mUIsRAAXgM0&t=6s)

A State Machine (HFSM) developed using Graph View, which operates on the default Animator principle. Unlike the Animator, we can assign the desired conditions for transitions through the interface. States and conditions can obtain their dependencies either locally or globally through a service locator. The advantage of this is that we can create both isolated states and isolated conditions.

The nice thing about the system is that since both conditions and states can operate in isolation, we have the opportunity to create a reusable state/condition library.

In summary, here are the differences from commonly used FSM systems:

- 🎯 The ability to add or remove states and transitions through the graph.
- ✨ Quickly create different state machines through the graph without writing code.
- 🔗 Since both states and conditions can receive the necessary dependencies via the service locator and the inspector, they can operate independently without an intermediary. 
- ♻️ In other words, there’s no need to place a Player class in between to access the animator, such as using Player.Animator. This way, the Player does not become a God Object over time, and the state is only dependent on the animator it requires; this allows for the state to be reusable.
- 🏛️ Support for Hierarchical Systems.
- 🧠 We can also add commonly used nodes in behavior trees like sequence, fallback, decorator, inverter, etc.
