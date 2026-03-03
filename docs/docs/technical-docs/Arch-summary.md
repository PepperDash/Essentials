# Essentials architecture

## Summary

PepperDash Essentials is an open-source framework for control systems, built on Crestron's Simpl# Pro framework. It can be configured as a standalone program capable of running a wide variety of system designs and can also be used to augment other Crestron programs.

Essentials is a collection of C# libraries that can be used in many ways. It is a 100% configuration-driven framework that can be extended to add different workflows and behaviors, either through the addition of new device-types and classes, or via a plug-in mechanism. The framework is a collection of things that are all related and interconnected, but in general do not have strong dependencies on each other.

## Framework Libraries

The table below is a guide to understand the basic organization of code concepts within the various libraries that make up the architecture.

![Table](~/docs/images/arch-table.PNG)

The diagram below shows the reference dependencies that exist between the different component libraries that make up the Essentials Framework.

![Architecture drawing](~/docs/images/arch-high-level.png)

Next: [Architecture](~/docs/technical-docs/Arch-1.md)
