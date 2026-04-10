# How to Add Documentation to Essentials

This guide explains how to add new documentation articles to the Essentials docFx site.

## Overview

The Essentials documentation uses [docFx](https://dotnet.github.io/docfx/) to generate a static documentation website. Documentation files are organized in a hierarchical structure with a table of contents (TOC) file that defines the site navigation. Documentation should be organized and written to fit into the [Diátaxis](https://diataxis.fr/start-here/) conceptual framework.

## Documentation Structure

Documentation files are located in `/docs/docs/` and organized into the following subdirectories:

- **how-to/** - Step-by-step guides and tutorials
- **usage/** - Usage documentation for SIMPL bridging, standalone use, and hardware integration
- **technical-docs/** - Technical documentation including architecture, plugins, and API references
- **images/** - Image assets used in documentation

## Adding a New Document

### Step 1: Create Your Markdown File

1. Determine which category your document belongs to (how-to, usage, or technical-docs)
2. Create a new `.md` file in the appropriate subdirectory
3. Use a descriptive filename with hyphens (e.g., `my-new-feature.md`)

**Example:**
```bash
# For a how-to guide
touch /docs/docs/how-to/configure-audio-settings.md

# For usage documentation
touch /docs/docs/usage/video-switcher-control.md

# For technical documentation
touch /docs/docs/technical-docs/custom-device-plugin.md
```

### Step 2: Write Your Content

Start your markdown file with a level 1 heading (`#`) that serves as the page title:

```markdown
# Your Document Title

Brief introduction to the topic.

## Section Heading

Content goes here...

### Subsection

More detailed content...
```

**Markdown Features:**
- Use standard markdown syntax
- Include code blocks with language specifiers (```csharp, ```json, etc.)
- Add images: `![Alt text](../images/your-image.png)`
- Link to other docs: `[Link text](../usage/related-doc.md)`

### Step 3: Add to Table of Contents

Edit `/docs/docs/toc.yml` to add your new document to the navigation:

```yaml
- name: How-to's
  items:
    - href: how-to/how-to-add-docs.md
    - href: how-to/your-new-doc.md  # Add your document here
```

**TOC Structure:**
- `name:` - Display name in the navigation menu
- `href:` - Relative path to the markdown file
- `items:` - Nested items for hierarchical navigation

**Example with nested items:**
```yaml
- name: Usage
  items:
  - name: SIMPL Bridging
    href: usage/SIMPL-Bridging-Updated.md
    items:
    - name: Your Sub-Topic
      href: usage/your-sub-topic.md
```

### Step 4: Test Locally

Build and preview the docFx site locally to verify your changes:

```bash
# Navigate to the docs directory
cd docs

# Build the documentation
docfx build

# Serve the site locally
docfx serve _site
```

Then open your browser to `http://localhost:8080` to view the site.

## Best Practices

### File Naming
- Use lowercase with hyphens: `my-document-name.md`
- Be descriptive but concise
- Avoid special characters

### Content Guidelines
- Start with a clear introduction
- Use hierarchical headings (H1 → H2 → H3)
- Include code examples where appropriate
- Add images to illustrate complex concepts
- Link to related documentation

### TOC Organization
- Group related documents under the same parent
- Order items logically (basic → advanced)
- Keep the TOC hierarchy shallow (2-3 levels max)
- Use clear, descriptive names for navigation items

## Common Issues

### Document Not Appearing
- Verify the file path in `toc.yml` is correct and uses forward slashes
- Ensure the markdown file exists in the specified location
- Check for YAML syntax errors in `toc.yml`

### Images Not Loading
- Verify image path is relative to the markdown file location
- Use `../images/` for files in the images directory
- Ensure image files are committed to the repository

### Broken Links
- Use relative paths for internal links
- Test all links after building the site
- Use `.md` extension when linking to other documentation files

## Additional Resources

- [docFx Documentation](https://dotnet.github.io/docfx/)
- [Markdown Guide](https://www.markdownguide.org/)
- [YAML Syntax](https://yaml.org/spec/1.2/spec.html)
- [Diátaxis](https://diataxis.fr/start-here/)
