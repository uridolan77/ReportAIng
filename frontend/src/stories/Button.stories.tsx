/**
 * Button Component Stories
 * 
 * Comprehensive Storybook stories for the Button component
 * showcasing all variants, states, and interactive features
 */

import type { Meta, StoryObj } from '@storybook/react';
import { action } from '@storybook/addon-actions';
import { Button } from 'antd';
import { 
  SearchOutlined, 
  DownloadOutlined, 
  DeleteOutlined,
  PlusOutlined,
  EditOutlined,
  SaveOutlined,
  LoadingOutlined
} from '@ant-design/icons';

const meta: Meta<typeof Button> = {
  title: 'Components/Button',
  component: Button,
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component: 'Ant Design Button component with comprehensive examples and variants.'
      }
    }
  },
  tags: ['autodocs'],
  argTypes: {
    type: {
      control: 'select',
      options: ['default', 'primary', 'dashed', 'text', 'link'],
      description: 'Button type'
    },
    size: {
      control: 'select',
      options: ['small', 'middle', 'large'],
      description: 'Button size'
    },
    shape: {
      control: 'select',
      options: ['default', 'circle', 'round'],
      description: 'Button shape'
    },
    disabled: {
      control: 'boolean',
      description: 'Disabled state'
    },
    loading: {
      control: 'boolean',
      description: 'Loading state'
    },
    danger: {
      control: 'boolean',
      description: 'Danger state'
    },
    ghost: {
      control: 'boolean',
      description: 'Ghost style'
    },
    block: {
      control: 'boolean',
      description: 'Full width'
    }
  },
  args: {
    onClick: action('clicked')
  }
};

export default meta;
type Story = StoryObj<typeof meta>;

// Basic button variants
export const Default: Story = {
  args: {
    children: 'Default Button'
  }
};

export const Primary: Story = {
  args: {
    type: 'primary',
    children: 'Primary Button'
  }
};

export const Dashed: Story = {
  args: {
    type: 'dashed',
    children: 'Dashed Button'
  }
};

export const Text: Story = {
  args: {
    type: 'text',
    children: 'Text Button'
  }
};

export const Link: Story = {
  args: {
    type: 'link',
    children: 'Link Button'
  }
};

// Button sizes
export const Small: Story = {
  args: {
    size: 'small',
    children: 'Small Button'
  }
};

export const Large: Story = {
  args: {
    size: 'large',
    children: 'Large Button'
  }
};

// Button shapes
export const Round: Story = {
  args: {
    shape: 'round',
    children: 'Round Button'
  }
};

export const Circle: Story = {
  args: {
    shape: 'circle',
    icon: <SearchOutlined />
  }
};

// Button states
export const Loading: Story = {
  args: {
    loading: true,
    children: 'Loading Button'
  }
};

export const Disabled: Story = {
  args: {
    disabled: true,
    children: 'Disabled Button'
  }
};

export const Danger: Story = {
  args: {
    danger: true,
    children: 'Danger Button'
  }
};

export const Ghost: Story = {
  args: {
    ghost: true,
    type: 'primary',
    children: 'Ghost Button'
  },
  parameters: {
    backgrounds: { default: 'dark' }
  }
};

export const Block: Story = {
  args: {
    block: true,
    type: 'primary',
    children: 'Block Button'
  }
};

// Buttons with icons
export const WithIcon: Story = {
  args: {
    icon: <SearchOutlined />,
    children: 'Search'
  }
};

export const IconOnly: Story = {
  args: {
    icon: <DownloadOutlined />,
    shape: 'circle'
  }
};

// Interactive examples
export const InteractiveExample: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
      <Button type="primary" icon={<PlusOutlined />} onClick={action('add-clicked')}>
        Add Item
      </Button>
      <Button icon={<EditOutlined />} onClick={action('edit-clicked')}>
        Edit
      </Button>
      <Button icon={<SaveOutlined />} onClick={action('save-clicked')}>
        Save
      </Button>
      <Button danger icon={<DeleteOutlined />} onClick={action('delete-clicked')}>
        Delete
      </Button>
    </div>
  )
};

// Loading states showcase
export const LoadingStates: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '8px', flexDirection: 'column', alignItems: 'flex-start' }}>
      <Button loading>Default Loading</Button>
      <Button type="primary" loading>
        Primary Loading
      </Button>
      <Button loading icon={<LoadingOutlined />}>
        Custom Loading Icon
      </Button>
      <Button shape="circle" loading />
    </div>
  )
};

// Size comparison
export const SizeComparison: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
      <Button size="small">Small</Button>
      <Button size="middle">Middle</Button>
      <Button size="large">Large</Button>
    </div>
  )
};

// Type comparison
export const TypeComparison: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
      <Button>Default</Button>
      <Button type="primary">Primary</Button>
      <Button type="dashed">Dashed</Button>
      <Button type="text">Text</Button>
      <Button type="link">Link</Button>
    </div>
  )
};

// Real-world usage examples
export const QueryActions: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
      <Button type="primary" icon={<SearchOutlined />}>
        Execute Query
      </Button>
      <Button icon={<SaveOutlined />}>
        Save Query
      </Button>
      <Button icon={<DownloadOutlined />}>
        Export Results
      </Button>
      <Button danger icon={<DeleteOutlined />}>
        Clear Query
      </Button>
    </div>
  )
};

export const FormActions: Story = {
  render: () => (
    <div style={{ display: 'flex', gap: '8px', justifyContent: 'flex-end' }}>
      <Button>Cancel</Button>
      <Button type="primary" icon={<SaveOutlined />}>
        Save Changes
      </Button>
    </div>
  )
};
