import React from 'react';
import { Card, CardContent } from '../ui';
import { Badge } from '../ui';
import { Link } from 'lucide-react';
import { SchemaRelationshipDto, SchemaTableContextDto } from '../../types/schemaManagement';

interface RelationshipEditorProps {
  relationships: SchemaRelationshipDto[];
  tableContexts: SchemaTableContextDto[];
  onUpdate: (updated: SchemaRelationshipDto) => void;
  onAdd: (newRelationship: SchemaRelationshipDto) => void;
}

export const RelationshipEditor: React.FC<RelationshipEditorProps> = ({
  relationships,
  tableContexts,
  onUpdate,
  onAdd
}) => {
  if (relationships.length === 0) {
    return (
      <div className="text-center py-8">
        <Link className="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">No Relationships</h3>
        <p className="text-gray-600">No table relationships defined for this schema version.</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {relationships.map((relationship) => (
        <Card key={relationship.id}>
          <CardContent className="p-4">
            <div className="flex items-start justify-between">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-2">
                  <h4 className="font-medium text-gray-900">
                    {relationship.fromTable} → {relationship.toTable}
                  </h4>
                  {relationship.isAutoGenerated && (
                    <Badge variant="secondary" className="text-xs">Auto</Badge>
                  )}
                  <Badge variant="outline" className="text-xs">
                    {relationship.relationshipType}
                  </Badge>
                </div>
                <p className="text-sm text-gray-600 mb-2">
                  {relationship.businessDescription || 'No description available'}
                </p>
                <div className="text-xs text-gray-500">
                  <span>Columns: {relationship.fromColumns.join(', ')} → {relationship.toColumns.join(', ')}</span>
                  {relationship.confidenceScore && (
                    <span className="ml-4">Confidence: {Math.round(relationship.confidenceScore * 100)}%</span>
                  )}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
};
