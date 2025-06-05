import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../ui';
import { GitCompare } from 'lucide-react';
import { BusinessSchemaDto, BusinessSchemaVersionDto } from '../../types/schemaManagement';

interface SchemaComparisonProps {
  schema: BusinessSchemaDto;
  currentVersion: BusinessSchemaVersionDto | null;
}

export const SchemaComparison: React.FC<SchemaComparisonProps> = ({
  schema,
  currentVersion
}) => {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <GitCompare className="h-5 w-5" />
          Schema Comparison
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="text-center py-8">
          <GitCompare className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            Schema Comparison
          </h3>
          <p className="text-gray-600">
            Compare different versions of your schema to track changes and improvements.
          </p>
          <p className="text-sm text-gray-500 mt-2">
            This feature will be available when multiple versions exist.
          </p>
        </div>
      </CardContent>
    </Card>
  );
};
