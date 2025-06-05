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
import { Upload } from 'lucide-react';
import { BusinessSchemaDto } from '../../types/schemaManagement';

interface ImportSchemaDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  schemas: BusinessSchemaDto[];
  onSchemaImported: () => void;
}

export const ImportSchemaDialog: React.FC<ImportSchemaDialogProps> = ({
  open,
  onOpenChange,
  schemas,
  onSchemaImported
}) => {
  const [loading, setLoading] = useState(false);

  const handleImport = async () => {
    // Placeholder for import functionality
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      onOpenChange(false);
      onSchemaImported();
    }, 1000);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Import Schema</DialogTitle>
          <DialogDescription>
            Import a schema from a previously exported file.
          </DialogDescription>
        </DialogHeader>

        <div className="text-center py-8">
          <Upload className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            Schema Import
          </h3>
          <p className="text-gray-600">
            Import functionality will be available in the next update.
          </p>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleImport} disabled={loading}>
            {loading ? 'Importing...' : 'Import'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
