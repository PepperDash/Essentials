# Contributors Guide

Essentials is an open source project. If you are interested in making it better,
there are many ways you can contribute. For example, you can:

- Submit a bug report
- Suggest a new feature
- Provide feedback by commenting on feature requests/proposals
- Propose a patch by submitting a pull request
- Suggest or submit documentation improvements
- Review outstanding pull requests
- Answer questions from other users
- Share the software with other users who are interested
- Teach others to use the software

## Bugs and Feature Requests

If you believe that you have found a bug or wish to propose a new feature,
please first search the existing [issues] to see if it has already been
reported. If you are unable to find an existing issue, consider using one of
the provided templates to create a new issue and provide as many details as you
can to assist in reproducing the bug or explaining your proposed feature.

## Patch Submission tips

Patches should be submitted in the form of Pull Requests to the Essentials
[repository] on GitHub. But first, consider the following tips to ensure a
smooth process when submitting a patch:

- Ensure that the patch compiles and does not break any build-time tests.
- Be understanding, patient, and friendly; developers may need time to review
  your submissions before they can take action or respond. This does not mean
  your contribution is not valued. If your contribution has not received a
  response in a reasonable time, consider commenting with a polite inquiry for
  an update.
- Limit your patches to the smallest reasonable change to achieve your intended
  goal. For example, do not make unnecessary indentation changes; but don't go
  out of your way to make the patch so minimal that it isn't easy to read,
  either. Consider the reviewer's perspective.
- Before submission, please squash your commits to using a message that starts
  with the issue number and a description of the changes.
- Isolate multiple patches from each other. If you wish to make several
  independent patches, do so in separate, smaller pull requests that can be
  reviewed more easily.
- Be prepared to answer questions from reviewers. They may have further
  questions before accepting your patch, and may even propose changes. Please
  accept this feedback constructively, and not as a rejection of your proposed
  change.
  
## GitFlow Branch Model
This repository adheres to the [GitFlow](https://nvie.com/posts/a-successful-git-branching-model/) branch model and is intitialized for GitFlow to make for consistent branch name prefixes.  Please take time to familiarize yourself with this model.

- `master` will contain the latest stable version of the framework and release builds will be created from tagged commits on `master`.
- HotFix/Patch Pull Requests should target `master` as the base branch.
- All other Pull Requests (bug fixes, enhancements, etc.) should target `development` as the base branch.
- `release/vX.Y.X` branches will be used for release candidates when moving new features from `development` to `master`.  
  Beta builds will be created from tagged commits on release candidate branches.

## Review

- We welcome code reviews from anyone. A committer is required to formally
  accept and merge the changes.
- Reviewers will be looking for things like threading issues, performance
  implications, API design, duplication of existing functionality, readability
  and code style, avoidance of bloat (scope-creep), etc.
- Reviewers will likely ask questions to better understand your change.
- Reviewers will make comments about changes to your patch:
    - MUST means that the change is required
    - SHOULD means that the change is suggested, further discussion on the
      subject may be required
    - COULD means that the change is optional

## Timeline and Managing Expectations

As we continue to engage contributors and learn best practices for running a successful open source project, our processes 
and guidance will likely evolve. We will try to communicate expectations as we are able and to always be responsive. We 
hope that the community will share their suggestions for improving this engagement.  Based on the level of initial interest 
we receive and the availability of resources to evaluate contributions, we anticipate the following:

- We will initially prioritize pull requests that include small bug fixes and code that addresses potential vulnerabilities   
  as well as pull requests that include improvements for processor language specifications because these require a 
  reasonable amount of effort to evaluate and will help us exercise and revise our process for accepting contributions.  In 
  other words, we are going to start small in order to work out the kinks first.
- We are committed to maintaining the integrity and security of our code base.  In addition to the careful review the 
  maintainers will give to code contributions to make sure they do not introduce new bugs or vulnerabilities, we will be 
  trying to identify best practices to incorporate with our open source project so that contributors can have more control 
  over whether their contributions are accepted. These might include things like style guides and requirements for tests and 
  documentation to accompany some code contributions.  As a result, it may take a long time for some contributions to be 
  accepted.  This does not mean we are ignoring them.
- We are committed to integrating this GitHub project with our team's regular development work flow so that the open source 
  project remains dynamic and relevant.  This may  affect our responsiveness and ability to accept pull requests 
  quickly.  This does not mean we are ignoring them.
- Not all innovative ideas need to be accepted as pull requests into this GitHub project to be valuable to the community.        
  There may be times when we recommend that you just share your code for some enhancement to Essentials from your own 
  repository. As we identify and recognize extensions that are of general interest to Essentials, we 
  may seek to incorporate them with our baseline.

## Legal

Consistent with Section D.6. of the GitHub Terms of Service as of 2019, and the MIT license, the project maintainer for this project accepts contributions using the inbound=outbound model.
When you submit a pull request to this repository (inbound), you are agreeing to license your contribution under the same terms as specified in [LICENSE] (outbound).

This is an open source project.
Contributions you make to this repository are completely voluntary.
When you submit an issue, bug report, question, enhancement, pull request, etc., you are offering your contribution without expectation of payment, you expressly waive any future pay claims against PepperDash related to your contribution, and you acknowledge that this does not create an obligation on the part of PepperDash of any kind.
Furthermore, your contributing to this project does not create an employer-employee relationship between the PepperDash and the contributor.

[issues]: https://github.com/PepperDash/Essentials/issues
[repository]: https://github.com/PepperDash/Essentials
[LICENSE]: https://github.com/PepperDash/Essentials/blob/master/LICENSE.md
