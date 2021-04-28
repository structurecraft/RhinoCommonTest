# StructureCraft Software Developer Test

Welcome to this software developer test repository.

Please complete the following tasks.

1. Fork the repo and keep it private, send me an invite to the private repo.

2. Create a new branch with your username.

3. Create a PR from develop -> new branch.

4. Please complete the following changes.

   1. Complete the static method Curves.DivideCurve.

      This method takes a curve and a series of planes. The planes must split the curve into its smallest loops. Note that the planes will all be parallel. See below for a visual explanation. The dotted grey outline is the input curve, the red dotted lines are planes and the solid rectangles are the resultant curves. The Unit tests will help you debug.

      ![diagram](C:\Users\csykes\Documents\cloned_gits\RhinoCommonTest\diagram.jpg)

   2. Complete the SplitCurvesCommand

      This command should take a users closed input curve and return split curves. You can choose how the user specifies the split points of the curve. The Command should work on all the Curves in the InputCurves.3dm file.

   3. Complete the GH SplitCurvesComponent to take the function and enable its use in Grasshopper.
   4. Add one Unit Test that ensures the static method Curves.DivideCurve throws an error if no planes are fed into the method.

5. Commit changes to the repo and message me via GitHub once you're done. You can make as many commits as you like.



# Rules

1. You can use a search engine and search for help.
2. You cannot post questions on site such as Discourse or StackOverflow looking for help, this is meant to be completed by you.
3. You can ask me questions on the PR and I will answer them.



# Hints

1. The grasshopper component will help you test how fast your code is, this algorithm will be run a lot and needs to be zippy.