import React, { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '../ui';
import { Button } from '../ui';
import { Input } from '../ui';
import { Textarea } from '../ui';
import { Label } from '../ui';
import { Badge } from '../ui';
import { Switch } from '../ui';
import { X, Plus } from 'lucide-react';
import { BusinessSchemaDto, CreateSchemaRequest } from '../../types/schemaManagement';
import { schemaManagementApi } from '../../services/schemaManagementApi';

interface CreateSchemaDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSchemaCreated: (schema: BusinessSchemaDto) => void;
}

export const CreateSchemaDialog: React.FC<CreateSchemaDialogProps> = ({
  open,
  onOpenChange,
  onSchemaCreated
}) => {
  const [formData, setFormData] = useState<CreateSchemaRequest & { setAsDefault: boolean }>({
    name: '',
    description: '',
    tags: [],
    setAsDefault: false
  });
  const [tagInput, setTagInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      tags: [],
      setAsDefault: false
    });
    setTagInput('');
    setErrors({});
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Schema name is required';
    } else if (formData.name.length < 3) {
      newErrors.name = 'Schema name must be at least 3 characters';
    } else if (formData.name.length > 100) {
      newErrors.name = 'Schema name must be less than 100 characters';
    }

    if (formData.description && formData.description.length > 500) {
      newErrors.description = 'Description must be less than 500 characters';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleAddTag = () => {
    const tag = tagInput.trim();
    if (tag && formData.tags && !formData.tags.includes(tag)) {
      setFormData(prev => ({
        ...prev,
        tags: [...(prev.tags || []), tag]
      }));
      setTagInput('');
    }
  };

  const handleRemoveTag = (tagToRemove: string) => {
    setFormData(prev => ({
      ...prev,
      tags: (prev.tags || []).filter(tag => tag !== tagToRemove)
    }));
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddTag();
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      setLoading(true);

      // Check if schema name already exists
      const nameExists = !(await schemaManagementApi.validateSchemaName(formData.name));
      if (nameExists) {
        setErrors({ name: 'A schema with this name already exists' });
        return;
      }

      // Create the schema
      const newSchema = await schemaManagementApi.createSchema({
        name: formData.name,
        description: formData.description || undefined,
        tags: (formData.tags && formData.tags.length > 0) ? formData.tags : undefined
      });

      // Set as default if requested
      if (formData.setAsDefault) {
        await schemaManagementApi.setDefaultSchema(newSchema.id);
        newSchema.isDefault = true;
      }

      onSchemaCreated(newSchema);
      onOpenChange(false);
      resetForm();
    } catch (error: any) {
      console.error('Error creating schema:', error);
      setErrors({ 
        submit: error.response?.data?.message || 'Failed to create schema. Please try again.' 
      });
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    onOpenChange(false);
    resetForm();
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Create New Schema</DialogTitle>
          <DialogDescription>
            Create a new business context schema to organize your database knowledge.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Schema Name */}
          <div className="space-y-2">
            <Label htmlFor="name">Schema Name *</Label>
            <Input
              id="name"
              value={formData.name}
              onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
              placeholder="e.g., Finance Analytics, Customer Data, Sales Reporting"
              className={errors.name ? 'border-red-500' : ''}
            />
            {errors.name && (
              <p className="text-sm text-red-600">{errors.name}</p>
            )}
          </div>

          {/* Description */}
          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Textarea
              id="description"
              value={formData.description}
              onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
              placeholder="Describe the purpose and scope of this schema..."
              rows={3}
              className={errors.description ? 'border-red-500' : ''}
            />
            {errors.description && (
              <p className="text-sm text-red-600">{errors.description}</p>
            )}
          </div>

          {/* Tags */}
          <div className="space-y-2">
            <Label htmlFor="tags">Tags</Label>
            <div className="flex gap-2">
              <Input
                id="tags"
                value={tagInput}
                onChange={(e) => setTagInput(e.target.value)}
                onKeyPress={handleKeyPress}
                placeholder="Add a tag and press Enter"
                className="flex-1"
              />
              <Button
                variant="outline"
                onClick={handleAddTag}
                disabled={!tagInput.trim()}
              >
                <Plus className="h-4 w-4" />
              </Button>
            </div>
            
            {formData.tags && formData.tags.length > 0 && (
              <div className="flex flex-wrap gap-1 mt-2">
                {formData.tags.map((tag, index) => (
                  <Badge key={index} variant="secondary" className="flex items-center gap-1">
                    {tag}
                    <button
                      type="button"
                      onClick={() => handleRemoveTag(tag)}
                      className="ml-1 hover:bg-gray-300 rounded-full p-0.5"
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                ))}
              </div>
            )}
          </div>

          {/* Set as Default */}
          <div className="flex items-center space-x-2">
            <Switch
              id="setAsDefault"
              checked={formData.setAsDefault}
              onChange={(checked: boolean) => setFormData(prev => ({ ...prev, setAsDefault: checked }))}
            />
            <Label htmlFor="setAsDefault" className="text-sm">
              Set as default schema for new users
            </Label>
          </div>

          {/* Submit Error */}
          {errors.submit && (
            <div className="p-3 bg-red-50 border border-red-200 rounded-md">
              <p className="text-sm text-red-600">{errors.submit}</p>
            </div>
          )}

          <DialogFooter>
            <Button
              variant="outline"
              onClick={handleCancel}
              disabled={loading}
            >
              Cancel
            </Button>
            <Button
              htmlType="submit"
              disabled={loading || !formData.name.trim()}
            >
              {loading ? 'Creating...' : 'Create Schema'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
};
