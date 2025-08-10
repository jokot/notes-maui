-- Migration: Add Tag and NoteTag tables
-- Date: 2024-01-01
-- Description: Add support for tagging notes with many-to-many relationship

-- Create Tag table
CREATE TABLE IF NOT EXISTS Tag (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    Color TEXT NOT NULL DEFAULT '#007ACC',
    UpdatedAt TEXT NOT NULL,
    UNIQUE(Name)
);

-- Create index on Tag.Name for faster lookups
CREATE INDEX IF NOT EXISTS IX_Tag_Name ON Tag(Name);

-- Create NoteTag junction table for many-to-many relationship
CREATE TABLE IF NOT EXISTS NoteTag (
    Id TEXT PRIMARY KEY,
    NoteId TEXT NOT NULL,
    TagId TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY (NoteId) REFERENCES Note(Id) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES Tag(Id) ON DELETE CASCADE,
    UNIQUE(NoteId, TagId)
);

-- Create indexes on NoteTag for faster queries
CREATE INDEX IF NOT EXISTS IX_NoteTag_NoteId ON NoteTag(NoteId);
CREATE INDEX IF NOT EXISTS IX_NoteTag_TagId ON NoteTag(TagId);
CREATE INDEX IF NOT EXISTS IX_NoteTag_NoteId_TagId ON NoteTag(NoteId, TagId);

-- Insert some default tags
INSERT OR IGNORE INTO Tag (Id, Name, Color, UpdatedAt) VALUES 
    (lower(hex(randomblob(16))), 'Work', '#007ACC', datetime('now')),
    (lower(hex(randomblob(16))), 'Personal', '#FF6B6B', datetime('now')),
    (lower(hex(randomblob(16))), 'Ideas', '#4ECDC4', datetime('now')),
    (lower(hex(randomblob(16))), 'Important', '#F7DC6F', datetime('now'));