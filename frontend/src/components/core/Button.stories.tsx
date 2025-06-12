/**
 * Button Component Stories
 * 
 * Comprehensive Storybook stories for the Button component showcasing
 * all variants, states, and use cases with interactive controls.
 */

// Optional Storybook imports - only available in development
import { Button, IconButton, ButtonGroup } from './Button';
import { Stack, Flex } from './Layout';
let Meta: any, StoryObj: any, action: any;
try {
  const storybook = require('@storybook/react');
  const addonActions = require('@storybook/addon-actions');
  Meta = storybook.Meta;
  StoryObj = storybook.StoryObj;
  action = addonActions.action;
} catch (e) {
  // Storybook not available - provide fallbacks
  Meta = {} as any;
  StoryObj = {} as any;
  action = () => () => {};
}

const meta: typeof Meta<typeof Button> = {
  title: 'Core Components/Button',
  component: Button,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component: `
The Button component is a versatile, accessible button with multiple variants and sizes.
It supports icons, loading states, and follows modern design patterns.

## Features
- Multiple variants (primary, secondary, outline, ghost, danger, success)
- Three sizes (small, medium, large)
- Icon support with positioning
- Loading and disabled states
- Full width option
- Accessibility compliant
- TypeScript support
        `,
      },
    },
  },
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: 'select',
      options: ['primary', 'secondary', 'outline', 'ghost', 'danger', 'success'],
      description: 'Visual style variant of the button',
    },
    size: {
      control: 'select',
      options: ['small', 'medium', 'large'],
      description: 'Size of the button',
    },
    fullWidth: {
      control: 'boolean',
      description: 'Whether the button should take full width of its container',
    },
    loading: {
      control: 'boolean',
      description: 'Whether the button is in loading state',
    },
    disabled: {
      control: 'boolean',
      description: 'Whether the button is disabled',
    },
    iconPosition: {
      control: 'select',
      options: ['left', 'right'],
      description: 'Position of the icon relative to text',
    },
    onClick: {
      action: 'clicked',
      description: 'Function called when button is clicked',
    },
  },
  args: {
    onClick: action('button-click'),
  },
};

export default meta;
type Story = typeof StoryObj<typeof Button>;

// Basic Stories
export const Default: Story = {
  args: {
    children: 'Default Button',
  },
};

export const Primary: Story = {
  args: {
    variant: 'primary',
    children: 'Primary Button',
  },
};

export const Secondary: Story = {
  args: {
    variant: 'secondary',
    children: 'Secondary Button',
  },
};

export const Outline: Story = {
  args: {
    variant: 'outline',
    children: 'Outline Button',
  },
};

export const Ghost: Story = {
  args: {
    variant: 'ghost',
    children: 'Ghost Button',
  },
};

export const Danger: Story = {
  args: {
    variant: 'danger',
    children: 'Danger Button',
  },
};

export const Success: Story = {
  args: {
    variant: 'success',
    children: 'Success Button',
  },
};

// Size Variants
export const Sizes: Story = {
  render: () => (
    <Stack spacing="md">
      <Button size="small">Small Button</Button>
      <Button size="medium">Medium Button</Button>
      <Button size="large">Large Button</Button>
    </Stack>
  ),
  parameters: {
    docs: {
      description: {
        story: 'Buttons come in three sizes: small, medium (default), and large.',
      },
    },
  },
};

// States
export const States: Story = {
  render: () => (
    <Stack spacing="md">
      <Button>Normal State</Button>
      <Button loading>Loading State</Button>
      <Button disabled>Disabled State</Button>
    </Stack>
  ),
  parameters: {
    docs: {
      description: {
        story: 'Buttons support different states including loading and disabled.',
      },
    },
  },
};

// With Icons
export const WithIcons: Story = {
  render: () => (
    <Stack spacing="md">
      <Button icon={<span>🚀</span>} iconPosition="left">
        Launch
      </Button>
      <Button icon={<span>→</span>} iconPosition="right">
        Next
      </Button>
      <Button icon={<span>📊</span>}>
        Icon Only Text
      </Button>
    </Stack>
  ),
  parameters: {
    docs: {
      description: {
        story: 'Buttons can include icons positioned on the left or right of the text.',
      },
    },
  },
};

// Full Width
export const FullWidth: Story = {
  args: {
    fullWidth: true,
    children: 'Full Width Button',
  },
  parameters: {
    layout: 'padded',
    docs: {
      description: {
        story: 'Buttons can be set to take the full width of their container.',
      },
    },
  },
};

// All Variants Showcase
export const AllVariants: Story = {
  render: () => (
    <Stack spacing="lg">
      <div>
        <h3 style={{ marginBottom: '16px' }}>Button Variants</h3>
        <Flex gap="md" wrap="wrap">
          <Button variant="primary">Primary</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="outline">Outline</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="danger">Danger</Button>
          <Button variant="success">Success</Button>
        </Flex>
      </div>
      
      <div>
        <h3 style={{ marginBottom: '16px' }}>Disabled States</h3>
        <Flex gap="md" wrap="wrap">
          <Button variant="primary" disabled>Primary</Button>
          <Button variant="secondary" disabled>Secondary</Button>
          <Button variant="outline" disabled>Outline</Button>
          <Button variant="ghost" disabled>Ghost</Button>
          <Button variant="danger" disabled>Danger</Button>
          <Button variant="success" disabled>Success</Button>
        </Flex>
      </div>
    </Stack>
  ),
  parameters: {
    layout: 'padded',
    docs: {
      description: {
        story: 'Complete showcase of all button variants in normal and disabled states.',
      },
    },
  },
};

// Icon Button Stories
export const IconButtonStory: Story = {
  render: () => (
    <Stack spacing="md">
      <Flex gap="md" align="center">
        <IconButton icon={<span>🔍</span>} aria-label="Search" />
        <IconButton icon={<span>❤️</span>} aria-label="Like" variant="primary" />
        <IconButton icon={<span>⚙️</span>} aria-label="Settings" variant="outline" />
        <IconButton icon={<span>🗑️</span>} aria-label="Delete" variant="danger" />
      </Flex>
      
      <div>
        <h4>Different Sizes</h4>
        <Flex gap="md" align="center">
          <IconButton icon={<span>📊</span>} aria-label="Chart" size="small" />
          <IconButton icon={<span>📊</span>} aria-label="Chart" size="medium" />
          <IconButton icon={<span>📊</span>} aria-label="Chart" size="large" />
        </Flex>
      </div>
    </Stack>
  ),
  parameters: {
    docs: {
      description: {
        story: 'Icon-only buttons for actions where space is limited. Always include aria-label for accessibility.',
      },
    },
  },
};

// Button Group Stories
export const ButtonGroupStory: Story = {
  render: () => (
    <Stack spacing="lg">
      <div>
        <h4>Horizontal Group</h4>
        <ButtonGroup variant="horizontal" spacing="normal">
          <Button variant="outline">First</Button>
          <Button variant="outline">Second</Button>
          <Button variant="outline">Third</Button>
        </ButtonGroup>
      </div>
      
      <div>
        <h4>Vertical Group</h4>
        <ButtonGroup variant="vertical" spacing="normal">
          <Button variant="outline">Option A</Button>
          <Button variant="outline">Option B</Button>
          <Button variant="outline">Option C</Button>
        </ButtonGroup>
      </div>
      
      <div>
        <h4>Different Spacing</h4>
        <Stack spacing="md">
          <ButtonGroup spacing="tight">
            <Button variant="outline">Tight</Button>
            <Button variant="outline">Spacing</Button>
          </ButtonGroup>
          <ButtonGroup spacing="normal">
            <Button variant="outline">Normal</Button>
            <Button variant="outline">Spacing</Button>
          </ButtonGroup>
          <ButtonGroup spacing="loose">
            <Button variant="outline">Loose</Button>
            <Button variant="outline">Spacing</Button>
          </ButtonGroup>
        </Stack>
      </div>
    </Stack>
  ),
  parameters: {
    docs: {
      description: {
        story: 'Button groups for related actions with configurable spacing and orientation.',
      },
    },
  },
};

// Interactive Example
export const Interactive: Story = {
  args: {
    variant: 'primary',
    size: 'medium',
    children: 'Interactive Button',
    fullWidth: false,
    loading: false,
    disabled: false,
  },
  parameters: {
    docs: {
      description: {
        story: 'Use the controls below to interact with the button and see how different props affect its appearance and behavior.',
      },
    },
  },
};
