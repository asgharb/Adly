﻿namespace Adly.Application.Contracts.FileService.Models;

public record SaveFileModel(string Base64File,string FileContent); //file content: image/png