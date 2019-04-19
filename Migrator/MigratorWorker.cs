﻿using Digimezzo.Utilities.IO;
using Knowte.Common.Base;
using Knowte.Common.Database;
using Knowte.Common.Database.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Migrator
{
    public class MigratorWorker
    {
        public MigratorWorker()
        {
        }

        public void Execute()
        {
            Console.WriteLine("Migrator");
            Console.WriteLine("========");

            // First check Roaming\NoteStudio
            string noteStudioFolder = Path.Combine(LegacyPaths.AppData(), "NoteStudio");

            // If Roaming\NoteStudio doesn't exist, check Local\NoteStudio
            if (!Directory.Exists(noteStudioFolder))
            {
                noteStudioFolder = Path.Combine(LegacyPaths.LocalAppData(), "NoteStudio");
            }

            string notesSubfolder = Path.Combine(LegacyPaths.AppData(), ProductInformation.ApplicationName, "Notes");
            var migrator = new DbMigrator(notesSubfolder);

            if (Directory.Exists(noteStudioFolder))
            {
                if (!migrator.DatabaseExists())
                {
                    // Create the database if it doesn't exist
                    migrator.InitializeNewDatabase();
                }

                int noteCount = 0;
                int notebookCount = 0;

                using (var conn = migrator.Factory.GetConnection())
                {
                    noteCount = conn.Table<Note>().Count();
                    notebookCount = conn.Table<Notebook>().Count();
                }

                if (noteCount == 0 & notebookCount == 0)
                {
                    Console.WriteLine(Environment.NewLine + "NoteStudio installation found! Do you want to start importing Notes and Notebooks? [Y/N]");

                    ConsoleKeyInfo info = Console.ReadKey();

                    if (info.Key == ConsoleKey.Y)
                    {
                        // Get the Notebooks from the xml
                        String noteStudioXmlFilePath = System.IO.Path.Combine(noteStudioFolder, "NoteStudio.xml");

                        if (File.Exists(noteStudioXmlFilePath))
                        {
                            XDocument doc = XDocument.Load(noteStudioXmlFilePath);

                            List<Notebook> notebooks = null;
                            List<Note> notes = null;
                            long newNoteCount = 0;

                            Console.WriteLine(Environment.NewLine + "Importing. Please wait...");

                            try
                            {
                                // Add to database
                                newNoteCount = (from t in doc.Element("NoteStudio").Elements("Counters")
                                                select Convert.ToInt64(t.Attribute("UntitledCount").Value)).FirstOrDefault();

                                notebooks = (from t in doc.Element("NoteStudio").Element("Notebooks").Elements("Notebook")
                                             select new Notebook()
                                             {
                                                 Id = t.Attribute("Id").Value,
                                                 Title = t.Attribute("Title").Value,
                                                 CreationDate = DateTime.Parse(t.Attribute("CreationDate").Value).Ticks,
                                             }).ToList();

                                notes = (from t in doc.Element("NoteStudio").Element("Notes").Elements("Note")
                                         select new Note()
                                         {
                                             NotebookId = t.Attribute("NotebookId").Value,
                                             Id = t.Attribute("Id").Value,
                                             Text = t.Attribute("Text") == null ? "" : t.Attribute("Text").Value,
                                             Title = t.Attribute("Title").Value,
                                             CreationDate = t.Attribute("CreationDate") == null ? DateTime.Now.Ticks : DateTime.Parse(t.Attribute("CreationDate").Value).Ticks,
                                             OpenDate = t.Attribute("OpenDate") == null ? DateTime.Now.Ticks : DateTime.Parse(t.Attribute("OpenDate").Value).Ticks,
                                             ModificationDate = t.Attribute("ModificationDate") == null ? DateTime.Now.Ticks : DateTime.Parse(t.Attribute("ModificationDate").Value).Ticks,
                                             Flagged = t.Attribute("Flagged") == null ? 0 : Convert.ToBoolean(t.Attribute("Flagged").Value) ? 1 : 0,
                                             Maximized = t.Attribute("Maximized") == null ? 0 : Convert.ToBoolean(t.Attribute("Maximized").Value) ? 1 : 0,
                                             Width = t.Attribute("Width") == null ? Defaults.DefaultNoteWidth : Convert.ToInt32(t.Attribute("Width").Value),
                                             Height = t.Attribute("Height") == null ? Defaults.DefaultNoteHeight : Convert.ToInt32(t.Attribute("Height").Value),
                                             Top = t.Attribute("Top") == null ? Defaults.DefaultNoteTop : Convert.ToInt32(t.Attribute("Top").Value),
                                             Left = t.Attribute("Left") == null ? Defaults.DefaultNoteLeft : Convert.ToInt32(t.Attribute("Left").Value)
                                         }).ToList();

                                using (var conn = migrator.Factory.GetConnection())
                                {
                                    // Notebooks
                                    foreach (Notebook notebook in notebooks)
                                    {
                                        conn.Insert(notebook);
                                    }

                                    // Notes
                                    foreach (Note note in notes)
                                    {
                                        conn.Insert(note);
                                    }
                                }

                                // Copy xaml files
                                string noteStudioNotesSubFolder = System.IO.Path.Combine(noteStudioFolder, "Notes");

                                if (Directory.Exists(noteStudioNotesSubFolder))
                                {
                                    // If the Notes subfolder doesn't exist, create it.
                                    if (!Directory.Exists(notesSubfolder))
                                    {
                                        Directory.CreateDirectory(notesSubfolder);
                                    }

                                    foreach (var file in Directory.GetFiles(noteStudioNotesSubFolder))
                                    {
                                        File.Copy(file, Path.Combine(notesSubfolder, Path.GetFileName(file)));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(Environment.NewLine + $"Import failed. Exception: {ex}");
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(Environment.NewLine + "Import complete!");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Environment.NewLine + "Import not allowed as the Knowte database is not empty.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            else
            {
                Console.WriteLine(Environment.NewLine + "NoteStudio installation not found.");
            }

            Console.WriteLine(Environment.NewLine + "Press any key to close this window...");
            Console.ReadKey();
        }
    }
}
