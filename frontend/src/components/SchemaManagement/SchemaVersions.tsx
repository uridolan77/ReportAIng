import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '../ui';
import { Badge } from '../ui';
import { Button } from '../ui';
import { History, Star, Calendar, User } from 'lucide-react';
import { BusinessSchemaDto, BusinessSchemaVersionDto } from '../../types/schemaManagement';

interface SchemaVersionsProps {
  schema: BusinessSchemaDto;
  selectedVersion: BusinessSchemaVersionDto | null;
  onVersionSelect: (version: BusinessSchemaVersionDto) => void;
  onVersionUpdated: () => void;
}

export const SchemaVersions: React.FC<SchemaVersionsProps> = ({
  schema,
  selectedVersion,
  onVersionSelect
}) => {
  // Mock versions for now - in real implementation, fetch from API
  const versions: BusinessSchemaVersionDto[] = schema.currentVersion ? [schema.currentVersion] : [];

  if (versions.length === 0) {
    return (
      <Card>
        <CardContent className="p-8 text-center">
          <History className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">No Versions</h3>
          <p className="text-gray-600">No versions available for this schema.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <span>Schema Versions</span>
          <Badge variant="secondary">{versions.length}</Badge>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          {versions.map((version) => (
            <div
              key={version.id}
              className={`p-4 border rounded-lg cursor-pointer transition-all hover:shadow-md ${
                selectedVersion?.id === version.id 
                  ? 'ring-2 ring-blue-500 bg-blue-50' 
                  : 'hover:bg-gray-50'
              }`}
              onClick={() => onVersionSelect(version)}
            >
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-2">
                    <h4 className="font-medium text-gray-900">
                      {version.versionName || `Version ${version.versionNumber}`}
                    </h4>
                    {version.isCurrent && (
                      <Badge variant="default" className="text-xs">
                        <Star className="h-3 w-3 mr-1" />
                        Current
                      </Badge>
                    )}
                    <Badge 
                      variant={version.isActive ? "default" : "secondary"}
                      className="text-xs"
                    >
                      {version.isActive ? "Active" : "Inactive"}
                    </Badge>
                  </div>

                  <p className="text-sm text-gray-600 mb-3">
                    {version.description || 'No description provided'}
                  </p>

                  <div className="flex items-center gap-4 text-xs text-gray-500">
                    <div className="flex items-center gap-1">
                      <User className="h-3 w-3" />
                      {version.createdBy}
                    </div>
                    <div className="flex items-center gap-1">
                      <Calendar className="h-3 w-3" />
                      {new Date(version.createdAt).toLocaleDateString()}
                    </div>
                  </div>
                </div>

                <div className="flex flex-col items-end gap-2">
                  <span className="text-sm font-medium text-gray-900">
                    v{version.versionNumber}
                  </span>
                  {!version.isCurrent && (
                    <Button size="small">
                      Set Current
                    </Button>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
};
